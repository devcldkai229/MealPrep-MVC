using System.Security.Claims;
using MealPrep.BLL.DTOs;
using MealPrep.BLL.Services;
using MealPrep.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPrep.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dto = await _dashboardService.GetDashboardDataAsync(userId);
        
        // Map DTO to ViewModel
        var vm = new DashboardVm
        {
            SubscriptionStatus = dto.SubscriptionStatus,
            NextDeliveryText = dto.NextDeliveryText,
            TodayCalories = dto.TodayCalories,
            FeaturedMeals = dto.FeaturedMeals,
            WeekCalories = dto.WeekCalories,
            RecentOrders = dto.RecentOrders
        };
        
        return View(vm);
    }
}
