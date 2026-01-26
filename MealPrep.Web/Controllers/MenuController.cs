using MealPrep.BLL.Services;
using MealPrep.DAL.Entities;
using MealPrep.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MealPrep.Web.Controllers;

[Authorize]
public class MenuController : Controller
{
    private readonly IMenuService _menuService;
    private readonly IAiMenuService _aiMenuService;
    private readonly IMealService _mealService;
    private readonly IUserService _userService;
    private readonly MealPrep.DAL.Data.AppDbContext _context;
    private readonly ILogger<MenuController> _logger;

    public MenuController(
        IMenuService menuService, 
        IAiMenuService aiMenuService,
        IMealService mealService,
        IUserService userService,
        MealPrep.DAL.Data.AppDbContext context,
        ILogger<MenuController> logger)
    {
        _menuService = menuService;
        _aiMenuService = aiMenuService;
        _mealService = mealService;
        _userService = userService;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> SelectMeals(DateOnly? startDate = null)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var dto = await _menuService.GetWeeklySelectionAsync(userId, startDate);
        
        if (dto == null)
        {
            TempData["ErrorMessage"] = "Bạn cần có đăng ký đang hoạt động để chọn món ăn.";
            return RedirectToAction("Index", "Subscription");
        }

        // Map DTO to ViewModel
        var vm = new WeeklySelectionVm
        {
            SubscriptionId = dto.SubscriptionId,
            MealsPerDay = dto.MealsPerDay,
            UserAllergies = dto.UserAllergies,
            DailySlots = dto.DailySlots.Select(ds => new DailySlot
            {
                Date = ds.Date,
                DayName = ds.DayName,
                DayOfWeek = ds.DayOfWeek,
                SlotsCount = ds.SlotsCount,
                AvailableMeals = ds.AvailableMeals.Select(mo => new MealOption
                {
                    MealId = mo.MealId,
                    Name = mo.Name,
                    Description = mo.Description,
                    ImageUrl = mo.ImageUrl,
                    Calories = mo.Calories,
                    Protein = mo.Protein,
                    Carbs = mo.Carbs,
                    Fat = mo.Fat,
                    HasAllergen = mo.HasAllergen,
                    AllergenWarning = mo.AllergenWarning
                }).ToList()
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("SelectMeals")]
    public async Task<IActionResult> SelectMealsPost()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Parse form data into selections
        var selections = new List<MealSelectionRequest>();
        var dateGroups = new Dictionary<string, MealSelectionRequest>();

        foreach (var key in Request.Form.Keys)
        {
            var match = Regex.Match(key, @"selections\[(\d+)\]\.(.+)");
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

        // Get subscription ID from form or get from service
        var dto = await _menuService.GetWeeklySelectionAsync(userId);
        if (dto == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy đăng ký đang hoạt động.";
            return RedirectToAction("Index", "Subscription");
        }

        // Map ViewModel selections to DTO
        var selectionDtos = selections.Select(s => new MealPrep.BLL.DTOs.MealSelectionRequestDto
        {
            Date = s.Date,
            SelectedMealIds = s.SelectedMealIds
        }).ToList();

        try
        {
            await _menuService.SaveMealSelectionsAsync(userId, dto.SubscriptionId, selectionDtos);
            TempData["SuccessMessage"] = "Đã lưu lựa chọn món ăn thành công!";
            return RedirectToAction("Index", "Dashboard");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to save meal selections");
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(SelectMeals));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save meal selections");
            TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu lựa chọn. Vui lòng thử lại.";
            return RedirectToAction(nameof(SelectMeals));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateAiMenu([FromForm] string? weeklyNotes)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            // Get active subscription
            var weeklySelection = await _menuService.GetWeeklySelectionAsync(userId);
            if (weeklySelection == null)
            {
                return Json(new { success = false, message = "Bạn cần có đăng ký đang hoạt động." });
            }

            // Use subscription StartDate as weekStart (not current Monday)
            // This ensures AI plans for the correct dates matching the subscription
            var weekStart = weeklySelection.DailySlots.FirstOrDefault()?.Date 
                ?? DateOnly.FromDateTime(DateTime.Today);

            // Get subscription details to validate date range
            var subscription = await _context.Set<Subscription>()
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == weeklySelection.SubscriptionId && s.AppUserId == userId);
            
            if (subscription == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin đăng ký." });
            }

            // Calculate weekEnd, ensuring it doesn't exceed subscription EndDate
            var weekEnd = weekStart.AddDays(6);
            if (subscription.EndDate.HasValue && weekEnd > subscription.EndDate.Value)
            {
                weekEnd = subscription.EndDate.Value;
            }

            // Generate AI recommendations with optional weekly notes
            // If weeklyNotes is provided, it will override profile.Notes
            var aiPlan = await _aiMenuService.GenerateMenuAsync(userId, weekStart, weeklyNotes);

            // Load all active meals for selection
            var allMeals = await _mealService.GetAllActiveMealsAsync();

            // Load user allergies
            var userAllergies = await _userService.GetUserAllergiesAsync(userId);

            // Build ViewModel
            var vm = new AiMenuRecommendationVm
            {
                SubscriptionId = weeklySelection.SubscriptionId,
                MealsPerDay = weeklySelection.MealsPerDay,
                UserAllergies = userAllergies,
                WeekStart = weekStart,
                WeekEnd = weekEnd
            };

            // Map AI recommendations
            var dayNames = new[] { "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy", "Chủ Nhật" };
            foreach (var dayPlan in aiPlan.OrderBy(d => d.day))
            {
                // Validate day range (1-7)
                if (dayPlan.day < 1 || dayPlan.day > 7)
                {
                    _logger.LogWarning("Invalid day from AI: {Day}, skipping", dayPlan.day);
                    continue;
                }

                var date = weekStart.AddDays(dayPlan.day - 1);
                
                // Validate meal IDs exist and count is exactly 2
                if (dayPlan.meal_ids.Count != 2)
                {
                    _logger.LogWarning("AI returned {Count} meals for day {Day}, expected exactly 2", 
                        dayPlan.meal_ids.Count, dayPlan.day);
                    // Truncate or pad to 2 meals
                    if (dayPlan.meal_ids.Count > 2)
                    {
                        dayPlan.meal_ids = dayPlan.meal_ids.Take(2).ToList();
                    }
                    else
                    {
                        _logger.LogError("AI returned less than 2 meals for day {Day}, cannot proceed", dayPlan.day);
                        continue; // Skip this day if less than 2 meals
                    }
                }
                
                var validMealIds = dayPlan.meal_ids.Where(id => allMeals.Any(m => m.Id == id)).ToList();
                if (validMealIds.Count != dayPlan.meal_ids.Count)
                {
                    var invalidIds = dayPlan.meal_ids.Except(validMealIds).ToList();
                    _logger.LogWarning("AI returned invalid meal IDs: {InvalidIds} for day {Day}", 
                        string.Join(", ", invalidIds), dayPlan.day);
                }
                
                // Ensure we have exactly 2 valid meals
                if (validMealIds.Count != 2)
                {
                    _logger.LogWarning("Day {Day} has {Count} valid meals, expected 2. Skipping.", 
                        dayPlan.day, validMealIds.Count);
                    continue;
                }

                var recommendedMeals = allMeals
                    .Where(m => validMealIds.Contains(m.Id))
                    .Select(m => 
                    {
                        // Parse Images JSON to get first image URL
                        string? imageUrl = null;
                        if (!string.IsNullOrEmpty(m.Images))
                        {
                            try
                            {
                                var images = System.Text.Json.JsonSerializer.Deserialize<List<string>>(m.Images);
                                imageUrl = images?.FirstOrDefault();
                            }
                            catch { }
                        }

                        return new MealOption
                        {
                            MealId = m.Id,
                            Name = m.Name,
                            Description = m.Description ?? "",
                            ImageUrl = imageUrl,
                            Calories = m.Calories,
                            Protein = m.Protein,
                            Carbs = m.Carbs,
                            Fat = m.Fat,
                            HasAllergen = _menuService.CheckAllergen(m.Ingredients ?? "", userAllergies),
                            AllergenWarning = _menuService.GetAllergenWarning(m.Ingredients ?? "", userAllergies)
                        };
                    })
                    .ToList();

                vm.AiRecommendations.Add(new AiDayRecommendation
                {
                    Day = dayPlan.day,
                    DayName = dayNames[dayPlan.day - 1], // Safe: already validated day is 1-7
                    Date = date,
                    RecommendedMealIds = validMealIds, // Use validated meal IDs
                    Reason = dayPlan.reason,
                    RecommendedMeals = recommendedMeals
                });
            }

            // Ensure we have recommendations for all 7 days
            // If AI returned fewer days, fill missing days with empty recommendations
            var existingDays = vm.AiRecommendations.Select(r => r.Day).ToHashSet();
            for (int day = 1; day <= 7; day++)
            {
                if (!existingDays.Contains(day))
                {
                    var date = weekStart.AddDays(day - 1);
                    vm.AiRecommendations.Add(new AiDayRecommendation
                    {
                        Day = day,
                        DayName = dayNames[day - 1],
                        Date = date,
                        RecommendedMealIds = new List<int>(),
                        Reason = "Chưa có đề xuất từ AI",
                        RecommendedMeals = new List<MealOption>()
                    });
                }
            }

            // Re-sort by day to ensure correct order
            vm.AiRecommendations = vm.AiRecommendations.OrderBy(r => r.Day).ToList();

            // Map all available meals
            vm.AvailableMeals = allMeals.Select(m => 
            {
                // Parse Images JSON to get first image URL
                string? imageUrl = null;
                if (!string.IsNullOrEmpty(m.Images))
                {
                    try
                    {
                        var images = System.Text.Json.JsonSerializer.Deserialize<List<string>>(m.Images);
                        imageUrl = images?.FirstOrDefault();
                    }
                    catch { }
                }

                return new MealOption
                {
                    MealId = m.Id,
                    Name = m.Name,
                    Description = m.Description ?? "",
                    ImageUrl = imageUrl,
                    Calories = m.Calories,
                    Protein = m.Protein,
                    Carbs = m.Carbs,
                    Fat = m.Fat,
                    HasAllergen = _menuService.CheckAllergen(m.Ingredients ?? "", userAllergies),
                    AllergenWarning = _menuService.GetAllergenWarning(m.Ingredients ?? "", userAllergies)
                };
            }).ToList();

            return PartialView("_AiMenuRecommendations", vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI menu");
            return Json(new { success = false, message = $"Lỗi khi tạo menu AI: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMealsForSelection(string? search = null, int page = 1, int pageSize = 4)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var (meals, totalCount) = await _menuService.GetMealsForSelectionAsync(search, page, pageSize, userId);
        
        return Json(new
        {
            meals = meals.Select(m => new
            {
                mealId = m.MealId,
                name = m.Name,
                description = m.Description,
                imageUrl = m.ImageUrl,
                calories = m.Calories,
                protein = m.Protein,
                carbs = m.Carbs,
                fat = m.Fat,
                hasAllergen = m.HasAllergen,
                allergenWarning = m.AllergenWarning
            }),
            totalCount = totalCount,
            page = page,
            pageSize = pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptAiMenu([FromBody] AcceptAiMenuRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            // Get active subscription
            var weeklySelection = await _menuService.GetWeeklySelectionAsync(userId);
            if (weeklySelection == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đăng ký đang hoạt động." });
            }

            // Get subscription to validate dates
            var subscription = await _context.Set<Subscription>()
                .FirstOrDefaultAsync(s => s.Id == weeklySelection.SubscriptionId && s.AppUserId == userId);
            
            if (subscription == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin đăng ký." });
            }

            // Convert AI recommendations to meal selections
            var selections = new List<MealPrep.BLL.DTOs.MealSelectionRequestDto>();
            foreach (var ds in request.DaySelections)
            {
                if (DateOnly.TryParse(ds.Date, out var date))
                {
                    // Validate date is within subscription period
                    if (date < subscription.StartDate)
                    {
                        _logger.LogWarning("Date {Date} is before subscription start {StartDate}, skipping", 
                            date, subscription.StartDate);
                        continue;
                    }
                    
                    if (subscription.EndDate.HasValue && date > subscription.EndDate.Value)
                    {
                        _logger.LogWarning("Date {Date} is after subscription end {EndDate}, skipping", 
                            date, subscription.EndDate);
                        continue;
                    }
                    
                    // Validate meal count doesn't exceed meals per day
                    if (ds.MealIds.Count > subscription.MealsPerDay)
                    {
                        _logger.LogWarning("Date {Date} has {Count} meals, but subscription allows only {MealsPerDay}, truncating", 
                            date, ds.MealIds.Count, subscription.MealsPerDay);
                        ds.MealIds = ds.MealIds.Take(subscription.MealsPerDay).ToList();
                    }
                    
                    selections.Add(new MealPrep.BLL.DTOs.MealSelectionRequestDto
                    {
                        Date = date,
                        SelectedMealIds = ds.MealIds
                    });
                }
            }
            
            if (selections.Count == 0)
            {
                return Json(new { success = false, message = "Không có lựa chọn hợp lệ nào được tìm thấy." });
            }

            // Save selections and create orders
            await _menuService.SaveMealSelectionsAsync(userId, weeklySelection.SubscriptionId, selections);

            return Json(new { success = true, message = "Đã lưu menu AI thành công!" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting AI menu");
            return Json(new { success = false, message = $"Lỗi khi lưu menu: {ex.Message}" });
        }
    }

}

public class AcceptAiMenuRequest
{
    public List<DaySelection> DaySelections { get; set; } = new();
}

public class DaySelection
{
    public string Date { get; set; } = string.Empty; // ISO format: "yyyy-MM-dd"
    public List<int> MealIds { get; set; } = new();
}
