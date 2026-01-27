using MealPrep.BLL.DTOs;
using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using MealPrep.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class MenuService : IMenuService
    {
        private readonly IRepository<Subscription> _subRepo;
        private readonly IRepository<WeeklyMenu> _menuRepo;
        private readonly IRepository<Meal> _mealRepo;
        private readonly IRepository<DeliveryOrder> _deliveryOrderRepo;
        private readonly IRepository<DeliveryOrderItem> _deliveryOrderItemRepo;
        private readonly IRepository<UserAllergy> _allergyRepo;
        private readonly IRepository<DeliverySlot> _slotRepo;
        private readonly IRepository<KitchenInventory> _inventoryRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<MenuService> _logger;

        public MenuService(
            IRepository<Subscription> subRepo,
            IRepository<WeeklyMenu> menuRepo,
            IRepository<Meal> mealRepo,
            IRepository<DeliveryOrder> deliveryOrderRepo,
            IRepository<DeliveryOrderItem> deliveryOrderItemRepo,
            IRepository<UserAllergy> allergyRepo,
            IRepository<DeliverySlot> slotRepo,
            IRepository<KitchenInventory> inventoryRepo,
            AppDbContext dbContext,
            ILogger<MenuService> logger)
        {
            _subRepo = subRepo;
            _menuRepo = menuRepo;
            _mealRepo = mealRepo;
            _deliveryOrderRepo = deliveryOrderRepo;
            _deliveryOrderItemRepo = deliveryOrderItemRepo;
            _allergyRepo = allergyRepo;
            _slotRepo = slotRepo;
            _inventoryRepo = inventoryRepo;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<WeeklySelectionDto?> GetWeeklySelectionAsync(Guid userId, DateOnly? startDate = null)
        {
            // Check if user has active subscription
            var activeSubscription = await _subRepo.Query()
                .Where(s => s.AppUserId == userId && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();

            if (activeSubscription == null)
            {
                return null;
            }

            // Use provided startDate or subscription StartDate, default to today if not provided
            DateOnly weekStart;
            if (startDate.HasValue)
            {
                weekStart = startDate.Value;
            }
            else
            {
                // Default to subscription StartDate, or today if subscription hasn't started
                weekStart = activeSubscription.StartDate > DateOnly.FromDateTime(DateTime.Today) 
                    ? activeSubscription.StartDate 
                    : DateOnly.FromDateTime(DateTime.Today);
            }

            // Ensure weekStart is within subscription period
            if (weekStart < activeSubscription.StartDate)
            {
                weekStart = activeSubscription.StartDate;
            }
            if (activeSubscription.EndDate.HasValue && weekStart > activeSubscription.EndDate.Value)
            {
                return null; // StartDate is after subscription end
            }

            var weekEnd = weekStart.AddDays(6);
            
            // Ensure weekEnd doesn't exceed subscription end
            if (activeSubscription.EndDate.HasValue && weekEnd > activeSubscription.EndDate.Value)
            {
                weekEnd = activeSubscription.EndDate.Value;
            }

            // Load weekly menu for current week
            var weeklyMenu = await _menuRepo.Query()
                .Include(m => m.Items)
                    .ThenInclude(i => i.Meal)
                .Where(m => m.WeekStart <= weekStart && m.WeekEnd >= weekEnd)
                .FirstOrDefaultAsync();

            if (weeklyMenu == null)
            {
                _logger.LogWarning("No weekly menu found for week {WeekStart} to {WeekEnd}, using all active meals", weekStart, weekEnd);
            }

            // Load user allergies
            var userAllergies = await _allergyRepo.Query()
                .Include(a => a.UserNutritionProfile)
                .Where(a => a.UserNutritionProfile!.AppUserId == userId)
                .Select(a => a.AllergyName.ToLower())
                .ToListAsync();

            // Load all active meals for fallback
            var allMeals = await _mealRepo.Query()
                .Where(m => m.IsActive)
                .ToListAsync();

            // Load existing DeliveryOrders for this subscription in the date range
            var existingOrders = await _deliveryOrderRepo.Query()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Meal)
                .Where(o => o.SubscriptionId == activeSubscription.Id &&
                           o.DeliveryDate >= weekStart &&
                           o.DeliveryDate <= weekEnd)
                .ToListAsync();

            var ordersByDate = existingOrders
                .Where(o => o.Items.Any()) // Only orders with items are considered "confirmed"
                .ToDictionary(o => o.DeliveryDate);

            var today = DateOnly.FromDateTime(DateTime.Today);

            // Load inventory data for all dates in the week (for performance)
            var weekDates = Enumerable.Range(0, 7)
                .Select(i => weekStart.AddDays(i))
                .Where(d => !activeSubscription.EndDate.HasValue || d <= activeSubscription.EndDate.Value)
                .ToList();
            
            var inventoryByDateAndMeal = await _inventoryRepo.Query()
                .Where(inv => weekDates.Contains(inv.Date))
                .ToListAsync();
            
            // Load used quantities (from DeliveryOrderItems) for all dates
            var usedQuantities = await _dbContext.Set<DeliveryOrderItem>()
                .Include(i => i.DeliveryOrder)
                .Where(i => weekDates.Contains(i.DeliveryOrder!.DeliveryDate) && i.MealId != null)
                .GroupBy(i => new { i.DeliveryOrder!.DeliveryDate, i.MealId })
                .Select(g => new { g.Key.DeliveryDate, g.Key.MealId, Used = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // Build daily slots - exactly 7 days from weekStart
            var dailySlots = new List<DailySlotDto>();
            var dayNames = new[] { "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật" };

            // Calculate number of days to show (max 7, or until subscription end)
            var daysToShow = 7;
            if (activeSubscription.EndDate.HasValue)
            {
                var daysUntilEnd = (activeSubscription.EndDate.Value.DayNumber - weekStart.DayNumber) + 1;
                daysToShow = Math.Min(7, daysUntilEnd);
            }

            for (int i = 0; i < daysToShow; i++)
            {
                var date = weekStart.AddDays(i);
                
                // Skip if date exceeds subscription end
                if (activeSubscription.EndDate.HasValue && date > activeSubscription.EndDate.Value)
                {
                    break;
                }
                
                var dayOfWeek = ((int)date.DayOfWeek + 6) % 7 + 1; // Convert to 1=Monday, 7=Sunday

                // Get meals for this day from weekly menu
                List<Meal> dayMenuItems;
                if (weeklyMenu != null)
                {
                    dayMenuItems = weeklyMenu.Items
                        .Where(item => item.DayOfWeek == dayOfWeek)
                        .Select(item => item.Meal)
                        .Where(m => m != null && m.IsActive)
                        .Cast<Meal>()
                        .ToList();

                    // If no meals in menu for this day, use all active meals
                    if (!dayMenuItems.Any())
                    {
                        dayMenuItems = allMeals;
                    }
                }
                else
                {
                    // No weekly menu, use all active meals
                    dayMenuItems = allMeals;
                }

                // Filter out meals with allergens (Hard Constraint: Zero Tolerance)
                var mealOptions = dayMenuItems
                    .Where(meal => !CheckAllergen(meal.Ingredients ?? "", userAllergies)) // Exclude meals with allergens
                    .Select(meal => 
                    {
                        // Parse Images JSON to get first image URL
                        string? imageUrl = null;
                        if (!string.IsNullOrEmpty(meal!.Images))
                        {
                            try
                            {
                                var images = JsonSerializer.Deserialize<List<string>>(meal.Images);
                                imageUrl = images?.FirstOrDefault();
                            }
                            catch
                            {
                                // If parse fails, ignore
                            }
                        }

                        // Check inventory availability for this meal and date
                        var inventory = inventoryByDateAndMeal.FirstOrDefault(inv => inv.Date == date && inv.MealId == meal.Id);
                        var usedQty = usedQuantities
                            .Where(u => u.DeliveryDate == date && u.MealId == meal.Id)
                            .Sum(u => u.Used);
                        
                        int? availableQty = null;
                        bool isSoldOut = false;
                        
                        if (inventory != null)
                        {
                            availableQty = Math.Max(0, inventory.QuantityLimit - usedQty);
                            isSoldOut = availableQty <= 0;
                        }

                        return new MealOptionDto
                        {
                            MealId = meal.Id,
                            Name = meal.Name,
                            Description = meal.Description ?? "",
                            ImageUrl = imageUrl,
                            Calories = meal.Calories,
                            Protein = meal.Protein,
                            Carbs = meal.Carbs,
                            Fat = meal.Fat,
                            HasAllergen = false, // Already filtered, so always false
                            AllergenWarning = "", // No warning needed since filtered
                            IsSoldOut = isSoldOut,
                            AvailableQuantity = availableQty
                        };
                    }).ToList();

                // Check if this date is locked (past date or has confirmed order)
                var hasOrder = ordersByDate.ContainsKey(date);
                var isPastDate = date < today;
                
                // Check cut-off time: Cannot edit tomorrow's meals after 20:00 today
                var tomorrow = today.AddDays(1);
                var now = DateTime.Now;
                var cutoffTime = new TimeOnly(20, 0); // 20:00
                var currentTime = TimeOnly.FromDateTime(now);
                var isPastCutoff = date == tomorrow && currentTime > cutoffTime;
                
                var isLocked = isPastDate || hasOrder || isPastCutoff;

                // Get locked meals info if order exists
                var lockedMeals = new List<LockedMealInfo>();
                if (hasOrder && ordersByDate[date].Items.Any())
                {
                    var orderItems = ordersByDate[date].Items.OrderBy(i => i.Id).ToList();
                    for (int slotIdx = 0; slotIdx < orderItems.Count && slotIdx < activeSubscription.MealsPerDay; slotIdx++)
                    {
                        var item = orderItems[slotIdx];
                        lockedMeals.Add(new LockedMealInfo
                        {
                            MealId = item.MealId ?? 0,
                            MealName = item.MealNameSnapshot,
                            SlotIndex = slotIdx
                        });
                    }
                }

                dailySlots.Add(new DailySlotDto
                {
                    Date = date,
                    DayName = date.ToString("dd/MM/yyyy"), // Use actual date format instead of day name
                    DayOfWeek = dayOfWeek,
                    AvailableMeals = mealOptions,
                    SlotsCount = activeSubscription.MealsPerDay,
                    IsLocked = isLocked,
                    HasOrder = hasOrder,
                    IsPastCutoff = isPastCutoff,
                    LockedMeals = lockedMeals
                });
            }

            return new WeeklySelectionDto
            {
                SubscriptionId = activeSubscription.Id,
                MealsPerDay = activeSubscription.MealsPerDay,
                DailySlots = dailySlots,
                UserAllergies = userAllergies
            };
        }

        public async Task<(List<MealOptionDto> Meals, int TotalCount)> GetMealsForSelectionAsync(string? search, int page, int pageSize, Guid userId, DateOnly? date = null)
        {
            // Load user allergies
            var userAllergies = await _allergyRepo.Query()
                .Include(a => a.UserNutritionProfile)
                .Where(a => a.UserNutritionProfile!.AppUserId == userId)
                .Select(a => a.AllergyName.ToLower())
                .ToListAsync();

            // Build query for active meals
            var query = _mealRepo.Query().Where(m => m.IsActive);

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Name.Contains(search) || (m.Description ?? "").Contains(search));
            }

            // Get all meals first (we need to filter allergens in memory)
            var allMeals = await query
                .OrderBy(m => m.Name)
                .ToListAsync();

            // Filter out meals with allergens (Hard Constraint: Zero Tolerance)
            var filteredMeals = allMeals
                .Where(meal => !CheckAllergen(meal.Ingredients ?? "", userAllergies))
                .ToList();

            // Get total count after filtering
            var totalCount = filteredMeals.Count;

            // Apply pagination
            var meals = filteredMeals
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Check inventory if date is provided
            DateOnly checkDate = date ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)); // Default to tomorrow
            var inventoryForDate = await _inventoryRepo.Query()
                .Where(inv => inv.Date == checkDate)
                .ToListAsync();
            
            var usedQuantities = await _dbContext.Set<DeliveryOrderItem>()
                .Include(i => i.DeliveryOrder)
                .Where(i => i.DeliveryOrder!.DeliveryDate == checkDate && i.MealId != null)
                .GroupBy(i => i.MealId)
                .Select(g => new { MealId = g.Key, Used = g.Sum(x => x.Quantity) })
                .ToListAsync();

            // Map to MealOptionDto
            var mealOptions = meals.Select(meal =>
            {
                // Parse Images JSON to get first image URL
                string? imageUrl = null;
                if (!string.IsNullOrEmpty(meal.Images))
                {
                    try
                    {
                        var images = JsonSerializer.Deserialize<List<string>>(meal.Images);
                        imageUrl = images?.FirstOrDefault();
                    }
                    catch { }
                }
                
                // Check inventory availability
                var inventory = inventoryForDate.FirstOrDefault(inv => inv.MealId == meal.Id);
                var usedQty = usedQuantities
                    .Where(u => u.MealId == meal.Id)
                    .Sum(u => u.Used);
                
                int? availableQty = null;
                bool isSoldOut = false;
                
                if (inventory != null)
                {
                    availableQty = Math.Max(0, inventory.QuantityLimit - usedQty);
                    isSoldOut = availableQty <= 0;
                }
                
                return new MealOptionDto
                {
                    MealId = meal.Id,
                    Name = meal.Name,
                    Description = meal.Description ?? "",
                    ImageUrl = imageUrl,
                    Calories = meal.Calories,
                    Protein = meal.Protein,
                    Carbs = meal.Carbs,
                    Fat = meal.Fat,
                    HasAllergen = false, // Already filtered, so always false
                    AllergenWarning = "", // No warning needed since filtered
                    IsSoldOut = isSoldOut,
                    AvailableQuantity = availableQty
                };
            }).ToList();

            return (mealOptions, totalCount);
        }

        public async Task<HashSet<DateOnly>> GetDatesWithConfirmedOrdersAsync(int subscriptionId, DateOnly fromDate, DateOnly toDate)
        {
            var existingOrders = await _deliveryOrderRepo.Query()
                .Include(o => o.Items)
                .Where(o => o.SubscriptionId == subscriptionId &&
                            o.DeliveryDate >= fromDate &&
                            o.DeliveryDate <= toDate)
                .ToListAsync();

            return existingOrders
                .Where(o => o.Items.Any())
                .Select(o => o.DeliveryDate)
                .ToHashSet();
        }

        public async Task SaveMealSelectionsAsync(Guid userId, int subscriptionId, List<MealSelectionRequestDto> selections)
        {
            // Verify active subscription
            var activeSubscription = await _subRepo.Query()
                .Include(s => s.AppUser)
                .Where(s => s.AppUserId == userId && s.Status == SubscriptionStatus.Active && s.Id == subscriptionId)
                .FirstOrDefaultAsync();

            if (activeSubscription == null)
            {
                throw new InvalidOperationException("Không tìm thấy đăng ký đang hoạt động.");
            }

            // Get user delivery address
            // Priority: 1. From DTO (if provided), 2. From User.DeliveryAddress, 3. Throw exception
            string? deliveryAddress = null;
            
            // Check if address is provided in any selection
            var addressFromDto = selections.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s.DeliveryAddress))?.DeliveryAddress;
            if (!string.IsNullOrWhiteSpace(addressFromDto))
            {
                deliveryAddress = addressFromDto.Trim();
                
                // If user doesn't have address yet, save it to user profile
                if (activeSubscription.AppUser != null && string.IsNullOrWhiteSpace(activeSubscription.AppUser.DeliveryAddress))
                {
                    activeSubscription.AppUser.DeliveryAddress = deliveryAddress;
                    // Update user in database
                    _dbContext.Set<AppUser>().Update(activeSubscription.AppUser);
                    _logger.LogInformation("Saving delivery address to user {UserId} profile: {Address}", userId, deliveryAddress);
                }
            }
            else if (!string.IsNullOrWhiteSpace(activeSubscription.AppUser?.DeliveryAddress))
            {
                deliveryAddress = activeSubscription.AppUser.DeliveryAddress.Trim();
            }
            
            // If still no address, throw exception to require user to provide it
            if (string.IsNullOrWhiteSpace(deliveryAddress))
            {
                throw new InvalidOperationException(
                    "Vui lòng cung cấp địa chỉ giao hàng. " +
                    "Bạn có thể cập nhật địa chỉ trong hồ sơ cá nhân hoặc nhập địa chỉ khi chọn món ăn.");
            }

            // Validate selections and check for locked orders
            var today = DateOnly.FromDateTime(DateTime.Today);
            var tomorrow = today.AddDays(1);
            var now = DateTime.Now;
            var cutoffTime = new TimeOnly(20, 0); // 20:00
            var currentTime = TimeOnly.FromDateTime(now);
            
            foreach (var selection in selections)
            {
                // Check if date is in the past
                if (selection.Date < today)
                {
                    throw new InvalidOperationException($"Không thể thay đổi món ăn cho ngày {selection.Date:dd/MM/yyyy} (ngày đã qua).");
                }

                // Check cut-off time: Cannot edit tomorrow's meals after 20:00 today
                if (selection.Date == tomorrow && currentTime > cutoffTime)
                {
                    throw new InvalidOperationException($"Không thể thay đổi món ăn cho ngày mai sau 20:00. Vui lòng liên hệ bếp để được hỗ trợ.");
                }

                // Check if date has a confirmed order (DeliveryOrder with items)
                var existingOrder = await _deliveryOrderRepo.Query()
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.SubscriptionId == activeSubscription.Id && 
                                             o.DeliveryDate == selection.Date);

                // If this date already has a confirmed order, skip it instead of blocking the whole save.
                // This allows user to save selections for other (future/unlocked) days.
                if (existingOrder != null && existingOrder.Items.Any())
                {
                    _logger.LogInformation(
                        "Skipping meal selection change for {Date} because a confirmed order already exists (OrderId={OrderId})",
                        selection.Date, existingOrder.Id);
                    continue;
                }

                if (selection.SelectedMealIds.Count > activeSubscription.MealsPerDay)
                {
                    throw new InvalidOperationException($"Ngày {selection.Date:dd/MM/yyyy}: Bạn chỉ được chọn tối đa {activeSubscription.MealsPerDay} món.");
                }
                
                // Validate slot concurrency: No duplicate meals in the same date
                var duplicateMeals = selection.SelectedMealIds
                    .GroupBy(id => id)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
                    
                if (duplicateMeals.Any())
                {
                    throw new InvalidOperationException($"Ngày {selection.Date:dd/MM/yyyy}: Không thể chọn cùng một món nhiều lần trong cùng một ngày.");
                }
                
                // Validate inventory availability for each meal
                foreach (var mealId in selection.SelectedMealIds)
                {
                    var inventory = await _inventoryRepo.Query()
                        .FirstOrDefaultAsync(inv => inv.Date == selection.Date && inv.MealId == mealId);
                    
                    if (inventory != null)
                    {
                        // Count how many are already ordered for this date and meal
                        var usedQty = await _dbContext.Set<DeliveryOrderItem>()
                            .Include(i => i.DeliveryOrder)
                            .Where(i => i.DeliveryOrder!.DeliveryDate == selection.Date && 
                                       i.MealId == mealId &&
                                       i.DeliveryOrderId != 0) // Exclude items from orders being updated
                            .SumAsync(i => i.Quantity);
                        
                        // Count how many this user is trying to order (including current selection)
                        var requestedQty = selection.SelectedMealIds.Count(id => id == mealId);
                        var availableQty = inventory.QuantityLimit - usedQty;
                        
                        if (requestedQty > availableQty)
                        {
                            var meal = await _mealRepo.GetByIdAsync(mealId);
                            var mealName = meal?.Name ?? $"Món #{mealId}";
                            throw new InvalidOperationException(
                                $"Ngày {selection.Date:dd/MM/yyyy}: Món '{mealName}' đã hết hàng. " +
                                $"Chỉ còn {availableQty} suất, nhưng bạn yêu cầu {requestedQty} suất.");
                        }
                    }
                }
            }

            // Validate meal existence and get meal details
            var allMealIds = selections.SelectMany(s => s.SelectedMealIds).Distinct().ToList();
            var meals = await _mealRepo.Query()
                .Where(m => allMealIds.Contains(m.Id) && m.IsActive)
                .ToListAsync();

            var invalidMealIds = allMealIds.Except(meals.Select(m => m.Id)).ToList();
            if (invalidMealIds.Any())
            {
                throw new InvalidOperationException($"Các món ăn sau không tồn tại hoặc đã bị vô hiệu hóa: {string.Join(", ", invalidMealIds)}");
            }

            var mealDict = meals.ToDictionary(m => m.Id);

            // Get default delivery slot (first active slot)
            var defaultSlot = await _slotRepo.Query()
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync();

            if (defaultSlot == null)
            {
                throw new InvalidOperationException("Không tìm thấy khung giờ giao hàng. Vui lòng liên hệ quản trị viên.");
            }

            // Calculate subscription period
            if (!activeSubscription.EndDate.HasValue)
            {
                throw new InvalidOperationException("Subscription chưa có EndDate. Vui lòng liên hệ quản trị viên.");
            }

            var subscriptionStart = activeSubscription.StartDate;
            var subscriptionEnd = activeSubscription.EndDate.Value;
            var subscriptionDays = (subscriptionEnd.DayNumber - subscriptionStart.DayNumber) + 1;

            // Start transaction
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Get date range from selections (if any) or use subscription period
                DateOnly weekStart, weekEnd;
                if (selections.Any())
                {
                    weekStart = selections.Min(s => s.Date);
                    weekEnd = selections.Max(s => s.Date);
                }
                else
                {
                    // If no selections, use subscription period
                    weekStart = subscriptionStart;
                    weekEnd = subscriptionEnd;
                }

                // Ensure we don't go outside subscription period
                if (weekStart < subscriptionStart) weekStart = subscriptionStart;
                if (weekEnd > subscriptionEnd) weekEnd = subscriptionEnd;

                // Delete existing delivery orders for this subscription in the date range
                // BUT only delete orders that don't have items (not confirmed yet)
                // Orders with items are locked and cannot be modified
                var existingOrders = await _deliveryOrderRepo.Query()
                    .Include(o => o.Items)
                    .Where(o => o.SubscriptionId == activeSubscription.Id &&
                               o.DeliveryDate >= weekStart &&
                               o.DeliveryDate <= weekEnd)
                    .ToListAsync();

                foreach (var order in existingOrders)
                {
                    // Only delete if order has no items (not confirmed)
                    // Orders with items are locked and should not be deleted
                    if (!order.Items.Any())
                    {
                        _deliveryOrderRepo.Remove(order);
                    }
                    else
                    {
                        // Order has items (confirmed), skip deletion
                        _logger.LogInformation("Skipping deletion of confirmed order {OrderId} for date {Date}", 
                            order.Id, order.DeliveryDate);
                    }
                }

                // Create/update delivery orders only for dates with selections
                // Skip dates that already have confirmed orders (with items)
                foreach (var selection in selections)
                {
                    var currentDate = selection.Date;
                    
                    // Check if this date already has a confirmed order (with items)
                    var existingOrder = existingOrders.FirstOrDefault(o => o.DeliveryDate == currentDate && o.Items.Any());
                    if (existingOrder != null)
                    {
                        _logger.LogWarning("Date {Date} already has confirmed order {OrderId}, skipping", 
                            currentDate, existingOrder.Id);
                        continue; // Skip dates with confirmed orders
                    }

                    // Check if there's an empty order (no items) - we'll update it
                    var emptyOrder = existingOrders.FirstOrDefault(o => o.DeliveryDate == currentDate && !o.Items.Any());
                    
                    decimal totalAmount = 0;
                    var orderItems = new List<DeliveryOrderItem>();

                    // Calculate total amount and create items for selected meals
                    foreach (var mealId in selection.SelectedMealIds)
                    {
                        if (!mealDict.TryGetValue(mealId, out var meal))
                        {
                            throw new InvalidOperationException($"Meal {mealId} not found or inactive");
                        }

                        var quantity = 1;
                        var unitPrice = meal.BasePrice;
                        totalAmount += unitPrice * quantity;

                        var orderItem = new DeliveryOrderItem
                        {
                            DeliveryOrderId = 0, // Will be set after order is created/updated
                            MealId = mealId,
                            MealNameSnapshot = meal.Name,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            DeliveryAddress = deliveryAddress // Save address snapshot for this delivery item
                        };
                        orderItems.Add(orderItem);
                    }

                    DeliveryOrder deliveryOrder;
                    if (emptyOrder != null)
                    {
                        // Update existing empty order
                        deliveryOrder = emptyOrder;
                        deliveryOrder.TotalAmount = totalAmount;
                        deliveryOrder.UpdatedAt = DateTime.UtcNow;
                        _deliveryOrderRepo.Update(deliveryOrder);
                    }
                    else
                    {
                        // Create new DeliveryOrder for this date
                        deliveryOrder = new DeliveryOrder
                        {
                            SubscriptionId = activeSubscription.Id,
                            DeliveryDate = currentDate,
                            DeliverySlotId = defaultSlot.Id,
                            Status = OrderStatus.Planned,
                            TotalAmount = totalAmount,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _deliveryOrderRepo.AddAsync(deliveryOrder);
                    }

                    await _dbContext.SaveChangesAsync(); // Save to get order ID

                    // Add items
                    if (orderItems.Any())
                    {
                        foreach (var item in orderItems)
                        {
                            item.DeliveryOrderId = deliveryOrder.Id;
                            await _deliveryOrderItemRepo.AddAsync(item);
                        }
                    }
                }

                // Save all changes within transaction
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Meal selections saved for user {UserId}, subscription {SubscriptionId}. Created delivery orders from {StartDate} to {EndDate}", 
                    userId, activeSubscription.Id, weekStart, weekEnd);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to save meal selections");
                throw;
            }
        }

        public bool CheckAllergen(string ingredients, List<string> userAllergies)
        {
            if (string.IsNullOrWhiteSpace(ingredients) || !userAllergies.Any())
                return false;

            try
            {
                var ingredientList = JsonSerializer.Deserialize<List<string>>(ingredients) ?? new List<string>();
                var ingredientsLower = ingredientList.Select(i => i.ToLower()).ToList();

                return userAllergies.Any(allergy =>
                    ingredientsLower.Any(ing => ing.Contains(allergy, StringComparison.OrdinalIgnoreCase)));
            }
            catch
            {
                // If JSON parsing fails, do simple string check
                var ingredientsLower = ingredients.ToLower();
                return userAllergies.Any(allergy => ingredientsLower.Contains(allergy));
            }
        }

        public string GetAllergenWarning(string ingredients, List<string> userAllergies)
        {
            if (string.IsNullOrWhiteSpace(ingredients) || !userAllergies.Any())
                return "";

            try
            {
                var ingredientList = JsonSerializer.Deserialize<List<string>>(ingredients) ?? new List<string>();
                var ingredientsLower = ingredientList.Select(i => i.ToLower()).ToList();

                var foundAllergies = userAllergies.Where(allergy =>
                    ingredientsLower.Any(ing => ing.Contains(allergy, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                return foundAllergies.Any()
                    ? $"⚠️ Có thể chứa: {string.Join(", ", foundAllergies)}"
                    : "";
            }
            catch
            {
                var ingredientsLower = ingredients.ToLower();
                var foundAllergies = userAllergies.Where(allergy => ingredientsLower.Contains(allergy)).ToList();
                return foundAllergies.Any()
                    ? $"⚠️ Có thể chứa: {string.Join(", ", foundAllergies)}"
                    : "";
            }
        }
    }
}
