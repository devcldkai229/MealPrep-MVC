using System.Security.Claims;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Repositories;
using MealPrep.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IRepository<Subscription> _subs;
    private readonly IRepository<Order> _orders;
    private readonly IRepository<NutritionLog> _logs;
    private readonly IRepository<Meal> _meals;

    public DashboardController(
        IRepository<Subscription> subs,
        IRepository<Order> orders,
        IRepository<NutritionLog> logs,
        IRepository<Meal> meals)
    {
        _subs = subs;
        _orders = orders;
        _logs = logs;
        _meals = meals;
    }

    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var activeSub = await _subs.Query()
            .Where(s => s.AppUserId == userId)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync();

        var nextOrder = await _orders.Query()
            .Include(o => o.DeliverySlot)
            .Where(o => o.AppUserId == userId && o.DeliveryDate >= today)
            .OrderBy(o => o.DeliveryDate)
            .FirstOrDefaultAsync();

        var todayCalories = await _logs.Query()
            .Include(l => l.Meal)
            .Where(l => l.AppUserId == userId && l.Date == today)
            .SumAsync(l => (l.Meal!.Calories * l.Quantity));

        // Featured meals: top 6 low-cal
        var featured = await _meals.Query()
            .Where(m => m.IsActive)
            .OrderBy(m => m.Calories)
            .Take(6)
            .ToListAsync();

        // Week summary: last 7 days calories
        var from = today.AddDays(-6);
        var weekLogs = await _logs.Query()
            .Include(l => l.Meal)
            .Where(l => l.AppUserId == userId && l.Date >= from && l.Date <= today)
            .ToListAsync();

        var week = weekLogs
            .GroupBy(x => x.Date)
            .Select(g => (g.Key, g.Sum(x => x.Meal!.Calories * x.Quantity)))
            .OrderBy(x => x.Key)
            .ToList();

        var recentOrders = await _orders.Query()
            .Include(o => o.DeliverySlot)
            .Include(o => o.Items).ThenInclude(i => i.Meal)
            .Where(o => o.AppUserId == userId)
            .OrderByDescending(o => o.DeliveryDate)
            .Take(5)
            .ToListAsync();

        var vm = new DashboardVm
        {
            SubscriptionStatus = activeSub?.Status.ToString() ?? "No subscription",
            NextDeliveryText = nextOrder == null ? "No upcoming delivery" : $"{nextOrder.DeliveryDate} • {nextOrder.DeliverySlot?.Name}",
            TodayCalories = todayCalories,
            FeaturedMeals = featured,
            WeekCalories = week,
            RecentOrders = recentOrders
        };

        return View(vm);
    }
}
