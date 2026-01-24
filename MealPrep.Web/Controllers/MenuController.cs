using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using MealPrep.DAL.Repositories;
using MealPrep.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace MealPrep.Web.Controllers;

[Authorize]
public class MenuController : Controller
{
    private readonly IRepository<Subscription> _subRepo;
    private readonly IRepository<WeeklyMenu> _menuRepo;
    private readonly IRepository<Meal> _mealRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<OrderItem> _orderItemRepo;
    private readonly IRepository<UserAllergy> _allergyRepo;
    private readonly IRepository<DeliverySlot> _slotRepo;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<MenuController> _logger;

    public MenuController(
        IRepository<Subscription> subRepo,
        IRepository<WeeklyMenu> menuRepo,
        IRepository<Meal> mealRepo,
        IRepository<Order> orderRepo,
        IRepository<OrderItem> orderItemRepo,
        IRepository<UserAllergy> allergyRepo,
        IRepository<DeliverySlot> slotRepo,
        AppDbContext dbContext,
        ILogger<MenuController> logger)
    {
        _subRepo = subRepo;
        _menuRepo = menuRepo;
        _mealRepo = mealRepo;
        _orderRepo = orderRepo;
        _orderItemRepo = orderItemRepo;
        _allergyRepo = allergyRepo;
        _slotRepo = slotRepo;
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> SelectMeals()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Check if user has active subscription
        var activeSubscription = await _subRepo.Query()
            .Where(s => s.AppUserId == userId && s.Status == SubscriptionStatus.Active)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync();

        if (activeSubscription == null)
        {
            TempData["ErrorMessage"] = "Bạn cần có đăng ký đang hoạt động để chọn món ăn.";
            return RedirectToAction("Index", "Subscription");
        }

        // Get current week (Monday to Sunday)
        var today = DateOnly.FromDateTime(DateTime.Today);
        var daysUntilMonday = ((int)today.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var weekStart = today.AddDays(-daysUntilMonday);
        var weekEnd = weekStart.AddDays(6);

        // Load weekly menu for current week
        var weeklyMenu = await _menuRepo.Query()
            .Include(m => m.Items)
                .ThenInclude(i => i.Meal)
            .Where(m => m.WeekStart <= weekStart && m.WeekEnd >= weekEnd)
            .FirstOrDefaultAsync();

        // If no weekly menu, use all active meals as fallback
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

        // Build daily slots
        var dailySlots = new List<DailySlot>();
        var dayNames = new[] { "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật" };

        for (int i = 0; i < 7; i++)
        {
            var date = weekStart.AddDays(i);
            var dayOfWeek = i + 1; // 1=Monday, 7=Sunday

            // Get meals for this day from weekly menu
            List<Meal> dayMenuItems;
            if (weeklyMenu != null)
            {
                dayMenuItems = weeklyMenu.Items
                    .Where(item => item.DayOfWeek == dayOfWeek)
                    .Select(item => item.Meal)
                    .Where(m => m != null && m.IsActive)
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

            var mealOptions = dayMenuItems.Select(meal => new MealOption
            {
                MealId = meal!.Id,
                Name = meal.Name,
                Description = meal.Description ?? "",
                Calories = meal.Calories,
                Protein = meal.Protein,
                Carbs = meal.Carbs,
                Fat = meal.Fat,
                HasAllergen = CheckAllergen(meal.Ingredients ?? "", userAllergies),
                AllergenWarning = GetAllergenWarning(meal.Ingredients ?? "", userAllergies)
            }).ToList();

            dailySlots.Add(new DailySlot
            {
                Date = date,
                DayName = dayNames[i],
                DayOfWeek = dayOfWeek,
                AvailableMeals = mealOptions,
                SlotsCount = activeSubscription.MealsPerDay
            });
        }

        var vm = new WeeklySelectionVm
        {
            SubscriptionId = activeSubscription.Id,
            MealsPerDay = activeSubscription.MealsPerDay,
            DailySlots = dailySlots,
            UserAllergies = userAllergies
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectMeals()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Verify active subscription
        var activeSubscription = await _subRepo.Query()
            .Where(s => s.AppUserId == userId && s.Status == SubscriptionStatus.Active)
            .FirstOrDefaultAsync();

        if (activeSubscription == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy đăng ký đang hoạt động.";
            return RedirectToAction("Index", "Subscription");
        }

        // Parse form data into selections
        var selections = new List<MealSelectionRequest>();
        var dateGroups = new Dictionary<string, MealSelectionRequest>();

        foreach (var key in Request.Form.Keys)
        {
            var match = System.Text.RegularExpressions.Regex.Match(key, @"selections\[(\d+)\]\.(.+)");
            if (match.Success)
            {
                var dayNumber = match.Groups[1].Value;
                var field = match.Groups[2].Value;

                if (!dateGroups.ContainsKey(dayNumber))
                {
                    dateGroups[dayNumber] = new MealSelectionRequest { SelectedMealIds = new List<int>() };
                }

                if (field == "Date")
                {
                    if (DateOnly.TryParse(Request.Form[key], out var date))
                    {
                        dateGroups[dayNumber].Date = date;
                    }
                }
                else if (field.StartsWith("SelectedMealIds[") && int.TryParse(Request.Form[key], out var mealId) && mealId > 0)
                {
                    if (!dateGroups[dayNumber].SelectedMealIds.Contains(mealId))
                    {
                        dateGroups[dayNumber].SelectedMealIds.Add(mealId);
                    }
                }
            }
        }

        selections = dateGroups.Values.Where(s => s.Date != default && s.SelectedMealIds.Any()).ToList();

        // Validate selections
        foreach (var selection in selections)
        {
            if (selection.SelectedMealIds.Count > activeSubscription.MealsPerDay)
            {
                TempData["ErrorMessage"] = $"Ngày {selection.Date:dd/MM/yyyy}: Bạn chỉ được chọn tối đa {activeSubscription.MealsPerDay} món.";
                return RedirectToAction(nameof(SelectMeals));
            }
        }

        // Get default delivery slot (first active slot)
        var defaultSlot = await _slotRepo.Query()
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync();

        if (defaultSlot == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy khung giờ giao hàng. Vui lòng liên hệ quản trị viên.";
            return RedirectToAction(nameof(SelectMeals));
        }

        // Start transaction
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Delete existing orders for this subscription in the selected week
            if (selections.Any())
            {
                var weekStart = selections.Min(s => s.Date);
                var weekEnd = selections.Max(s => s.Date);

                var existingOrders = await _orderRepo.Query()
                    .Include(o => o.Items)
                    .Where(o => o.SubscriptionId == activeSubscription.Id &&
                               o.DeliveryDate >= weekStart &&
                               o.DeliveryDate <= weekEnd)
                    .ToListAsync();

                foreach (var order in existingOrders)
                {
                    foreach (var item in order.Items)
                    {
                        _orderItemRepo.Remove(item);
                    }
                    _orderRepo.Remove(order);
                }
            }

            // Create new orders
            foreach (var selection in selections)
            {
                if (selection.SelectedMealIds.Any())
                {
                    var order = new Order
                    {
                        AppUserId = userId,
                        SubscriptionId = activeSubscription.Id,
                        DeliveryDate = selection.Date,
                        DeliverySlotId = defaultSlot.Id,
                        Status = OrderStatus.Planned
                    };

                    await _orderRepo.AddAsync(order);
                    
                    // Create order items
                    foreach (var mealId in selection.SelectedMealIds)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            MealId = mealId,
                            Quantity = 1
                        };
                        await _orderItemRepo.AddAsync(orderItem);
                    }
                }
            }

            // Save all changes within transaction
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Meal selections saved for user {UserId}, subscription {SubscriptionId}", userId, activeSubscription.Id);

            TempData["SuccessMessage"] = "Đã lưu lựa chọn món ăn thành công!";
            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to save meal selections");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu lựa chọn. Vui lòng thử lại.";
            return RedirectToAction(nameof(SelectMeals));
        }
    }

    private bool CheckAllergen(string ingredients, List<string> userAllergies)
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

    private string GetAllergenWarning(string ingredients, List<string> userAllergies)
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
