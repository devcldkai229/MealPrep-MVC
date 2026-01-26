using MealPrep.DAL.Entities;

namespace MealPrep.BLL.DTOs;

public class DashboardDto
{
    public string? SubscriptionStatus { get; set; }
    public string? NextDeliveryText { get; set; }
    public int TodayCalories { get; set; }

    public List<Meal> FeaturedMeals { get; set; } = new();
    public List<(DateOnly Date, int Calories)> WeekCalories { get; set; } = new();

    public List<Order> RecentOrders { get; set; } = new();
}
