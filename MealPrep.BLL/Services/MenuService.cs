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

            // Use provided startDate or subscription StartDate, default to tomorrow if subscription starts today or earlier
            DateOnly weekStart;
            var today = DateOnly.FromDateTime(DateTime.Today);
            var tomorrow = today.AddDays(1);
            
            if (startDate.HasValue)
            {
                weekStart = startDate.Value;
            }
            else
            {
                // If subscription StartDate is today or earlier, start from tomorrow
                // Otherwise, use subscription StartDate
                if (activeSubscription.StartDate <= today)
                {
                    // Subscription already started or starts today, begin from tomorrow
                    weekStart = tomorrow;
                }
                else
                {
                    // Subscription starts in the future, use StartDate
                    weekStart = activeSubscription.StartDate;
                }
            }

            // Ensure weekStart is within subscription period
            // Special case: If subscription StartDate is today or earlier, always start from tomorrow
            if (activeSubscription.StartDate <= today)
            {
                // Force start from tomorrow if subscription already started or starts today
                if (weekStart <= today)
                {
                    weekStart = tomorrow;
                }
            }
            else
            {
                // Subscription starts in the future, ensure weekStart is not before StartDate
                if (weekStart < activeSubscription.StartDate)
                {
                    weekStart = activeSubscription.StartDate;
                }
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
                .Include(o => o.Items)
                    .ThenInclude(i => i.DeliverySlot)
                .Where(o => o.SubscriptionId == activeSubscription.Id &&
                           o.DeliveryDate >= weekStart &&
                           o.DeliveryDate <= weekEnd)
                .ToListAsync();

            var ordersByDate = existingOrders
                .Where(o => o.Items.Any()) // Only orders with items are considered "confirmed"
                .ToDictionary(o => o.DeliveryDate);

            // today is already defined above

            // Build daily slots - exactly 7 days from weekStart (không áp dụng giới hạn kho tạm thời)
            var dailySlots = new List<DailySlotDto>();
            var dayNames = new[] { "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật" };

            // Calculate number of days to show
            // Always try to show 7 days from weekStart, but respect subscription EndDate if it's earlier
            var daysToShow = 7;
            if (activeSubscription.EndDate.HasValue)
            {
                var daysUntilEnd = (activeSubscription.EndDate.Value.DayNumber - weekStart.DayNumber) + 1;
                // Ensure we show at least the days available, up to 7 days
                daysToShow = Math.Min(7, Math.Max(1, daysUntilEnd));
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

                        // Tạm thời không áp dụng logic kho: luôn cho phép chọn món, không giới hạn số suất
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
                            IsSoldOut = false,
                            AvailableQuantity = null
                        };
                    }).ToList();

                // Check if this date is locked
                // 1) Past or current day based on midnight (00:00) cutoff
                //    => Khi đã sang ngày N, không thể chỉnh sửa các bữa của ngày N hoặc trước đó.
                var hasOrder = ordersByDate.ContainsKey(date);
                var now = DateTime.Now;
                var dateMidnight = date.ToDateTime(TimeOnly.MinValue);
                var isPastCutoff = now >= dateMidnight; // đã qua 00:00 của ngày delivery
                var isPastDate = date < today; // vẫn giữ để phân biệt ở UI nếu cần

                var isLocked = isPastDate || hasOrder || isPastCutoff;

                // Get locked meals info if order exists
                // Map items to slot index based on DeliverySlotId: Morning (Id=1) = 0, Evening (Id=3) = 1
                var lockedMeals = new List<LockedMealInfo>();
                if (hasOrder && ordersByDate[date].Items.Any())
                {
                    var orderItems = ordersByDate[date].Items
                        .Where(i => i.DeliverySlotId.HasValue)
                        .OrderBy(i => i.DeliverySlotId == 1 ? 0 : 1) // Morning (Id=1) trước, Evening (Id=3) sau
                        .ThenBy(i => i.Id)
                        .ToList();
                    
                    foreach (var item in orderItems)
                    {
                        // Map DeliverySlotId to SlotIndex: Morning (Id=1) = 0, Evening (Id=3) = 1
                        int slotIndex = item.DeliverySlotId == 1 ? 0 : (item.DeliverySlotId == 3 ? 1 : -1);
                        if (slotIndex >= 0 && slotIndex < activeSubscription.MealsPerDay)
                        {
                            lockedMeals.Add(new LockedMealInfo
                            {
                                MealId = item.MealId ?? 0,
                                MealName = item.MealNameSnapshot,
                                SlotIndex = slotIndex
                            });
                        }
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
                    IsSoldOut = false,
                    AvailableQuantity = null
                };
            }).ToList();

            return (mealOptions, totalCount);
        }

        public async Task<HashSet<DateOnly>> GetDatesWithConfirmedOrdersAsync(int subscriptionId, DateOnly fromDate, DateOnly toDate)
        {
            var existingOrders = await _deliveryOrderRepo.Query()
                .Include(o => o.Items)
                    .ThenInclude(i => i.DeliverySlot)
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
            
            foreach (var selection in selections)
            {
                // Không cho phép chỉnh sửa các ngày trong quá khứ
                if (selection.Date < today)
                {
                    throw new InvalidOperationException($"Không thể thay đổi món ăn cho ngày {selection.Date:dd/MM/yyyy} (ngày đã qua).");
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
                
                // Không kiểm tra tồn kho tạm thời – cho phép chọn bất kỳ món nào còn active
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

            // Get active delivery slots: chỉ lấy Morning và Evening (theo thứ tự: Morning trước, Evening sau)
            var activeSlots = await _slotRepo.Query()
                .Where(s => s.IsActive && (s.Name == "Morning" || s.Name == "Evening"))
                .OrderBy(s => s.Name == "Morning" ? 0 : 1) // Morning trước, Evening sau
                .ToListAsync();

            if (activeSlots == null || !activeSlots.Any() || activeSlots.Count < 2)
            {
                throw new InvalidOperationException("Không tìm thấy đủ khung giờ giao hàng (cần Morning và Evening). Vui lòng liên hệ quản trị viên.");
            }

            // Map slot index to slot: 0 = Morning, 1 = Evening
            // Đảm bảo chỉ có 2 slots: Morning và Evening
            var morningSlot = activeSlots.FirstOrDefault(s => s.Name == "Morning");
            var eveningSlot = activeSlots.FirstOrDefault(s => s.Name == "Evening");

            if (morningSlot == null || eveningSlot == null)
            {
                throw new InvalidOperationException("Hệ thống cần có cả Morning và Evening slot. Vui lòng liên hệ quản trị viên.");
            }

            var slotMap = new Dictionary<int, DeliverySlot>
            {
                { 0, morningSlot }, // Slot index 0 = Morning
                { 1, eveningSlot }  // Slot index 1 = Evening
            };

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
                        .ThenInclude(i => i.DeliverySlot)
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

                    // Process selections: Use SelectedMealsBySlot if available, otherwise fallback to SelectedMealIds
                    var mealsToProcess = new List<(int MealId, int SlotIndex)>();
                    
                    if (selection.SelectedMealsBySlot != null && selection.SelectedMealsBySlot.Any())
                    {
                        // New format: Dictionary<SlotIndex, MealId>
                        foreach (var kvp in selection.SelectedMealsBySlot)
                        {
                            mealsToProcess.Add((kvp.Value, kvp.Key));
                        }
                    }
                    else if (selection.SelectedMealIds != null && selection.SelectedMealIds.Any())
                    {
                        // Legacy format: List of MealIds (assign to slots in order)
                        for (int i = 0; i < selection.SelectedMealIds.Count; i++)
                        {
                            mealsToProcess.Add((selection.SelectedMealIds[i], i));
                        }
                    }

                    // Calculate total amount and create items for selected meals with slot assignment
                    foreach (var (mealId, slotIndex) in mealsToProcess)
                    {
                        if (!mealDict.TryGetValue(mealId, out var meal))
                        {
                            throw new InvalidOperationException($"Meal {mealId} not found or inactive");
                        }

                        // Get slot for this index (0 = Morning, 1 = Evening)
                        if (!slotMap.TryGetValue(slotIndex, out var slot))
                        {
                            // If slot index is invalid, default to Morning (index 0)
                            if (slotIndex < 0)
                            {
                                slot = slotMap[0]; // Default to Morning
                                _logger.LogWarning("Invalid SlotIndex {SlotIndex}, defaulting to Morning", slotIndex);
                            }
                            else
                            {
                                // If slot index exceeds available slots (shouldn't happen with 2 slots), use Evening
                                slot = slotMap[1]; // Default to Evening
                                _logger.LogWarning("SlotIndex {SlotIndex} exceeds available slots, using Evening", slotIndex);
                            }
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
                            DeliverySlotId = slot.Id, // Assign slot to item
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
                        // Create new DeliveryOrder for this date (no DeliverySlotId at order level anymore)
                        deliveryOrder = new DeliveryOrder
                        {
                            SubscriptionId = activeSubscription.Id,
                            DeliveryDate = currentDate,
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
