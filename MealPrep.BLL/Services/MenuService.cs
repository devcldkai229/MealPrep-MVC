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

                var mealOptions = dayMenuItems.Select(meal => 
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
                        HasAllergen = CheckAllergen(meal.Ingredients ?? "", userAllergies),
                        AllergenWarning = GetAllergenWarning(meal.Ingredients ?? "", userAllergies)
                    };
                }).ToList();

                dailySlots.Add(new DailySlotDto
                {
                    Date = date,
                    DayName = date.ToString("dd/MM/yyyy"), // Use actual date format instead of day name
                    DayOfWeek = dayOfWeek,
                    AvailableMeals = mealOptions,
                    SlotsCount = activeSubscription.MealsPerDay
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

        public async Task<(List<MealOptionDto> Meals, int TotalCount)> GetMealsForSelectionAsync(string? search, int page, int pageSize, Guid userId)
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

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var meals = await query
                .OrderBy(m => m.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                    HasAllergen = CheckAllergen(meal.Ingredients ?? "", userAllergies),
                    AllergenWarning = GetAllergenWarning(meal.Ingredients ?? "", userAllergies)
                };
            }).ToList();

            return (mealOptions, totalCount);
        }

        public async Task SaveMealSelectionsAsync(Guid userId, int subscriptionId, List<MealSelectionRequestDto> selections)
        {
            // Verify active subscription
            var activeSubscription = await _subRepo.Query()
                .Where(s => s.AppUserId == userId && s.Status == SubscriptionStatus.Active && s.Id == subscriptionId)
                .FirstOrDefaultAsync();

            if (activeSubscription == null)
            {
                throw new InvalidOperationException("Không tìm thấy đăng ký đang hoạt động.");
            }

            // Validate selections
            foreach (var selection in selections)
            {
                if (selection.SelectedMealIds.Count > activeSubscription.MealsPerDay)
                {
                    throw new InvalidOperationException($"Ngày {selection.Date:dd/MM/yyyy}: Bạn chỉ được chọn tối đa {activeSubscription.MealsPerDay} món.");
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
                var existingOrders = await _deliveryOrderRepo.Query()
                    .Include(o => o.Items)
                    .Where(o => o.SubscriptionId == activeSubscription.Id &&
                               o.DeliveryDate >= weekStart &&
                               o.DeliveryDate <= weekEnd)
                    .ToListAsync();

                foreach (var order in existingOrders)
                {
                    foreach (var item in order.Items)
                    {
                        _deliveryOrderItemRepo.Remove(item);
                    }
                    _deliveryOrderRepo.Remove(order);
                }

                // Create a dictionary of selections by date for quick lookup
                var selectionsByDate = selections.ToDictionary(s => s.Date, s => s.SelectedMealIds);

                // Create delivery orders for ALL days in the subscription period (or selected week)
                for (int i = 0; i <= (weekEnd.DayNumber - weekStart.DayNumber); i++)
                {
                    var currentDate = weekStart.AddDays(i);
                    
                    // Check if user has selected meals for this date
                    var hasSelections = selectionsByDate.TryGetValue(currentDate, out var selectedMealIds) && selectedMealIds.Any();

                    decimal totalAmount = 0;
                    var orderItems = new List<DeliveryOrderItem>();

                    if (hasSelections && selectedMealIds != null)
                    {
                        // Calculate total amount and create items for selected meals
                        foreach (var mealId in selectedMealIds)
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
                                DeliveryOrderId = 0, // Will be set after order is created
                                MealId = mealId,
                                MealNameSnapshot = meal.Name,
                                Quantity = quantity,
                                UnitPrice = unitPrice
                            };
                            orderItems.Add(orderItem);
                        }
                    }

                    // Create DeliveryOrder for this date (even if no meals selected)
                    var deliveryOrder = new DeliveryOrder
                    {
                        SubscriptionId = activeSubscription.Id,
                        DeliveryDate = currentDate,
                        DeliverySlotId = defaultSlot.Id,
                        Status = OrderStatus.Planned,
                        TotalAmount = totalAmount, // 0 if no meals selected
                        CreatedAt = DateTime.UtcNow
                    };

                    await _deliveryOrderRepo.AddAsync(deliveryOrder);
                    await _dbContext.SaveChangesAsync(); // Save to get order ID

                    // Add items if any
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
