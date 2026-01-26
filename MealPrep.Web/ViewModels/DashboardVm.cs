using MealPrep.BLL.Services;
using MealPrep.DAL.Entities;
using System;
using System.Collections.Generic;

namespace MealPrep.Web.ViewModels
{
    public class DashboardVm
    {
        public string SubscriptionStatus { get; set; } = string.Empty;
        public string NextDeliveryText { get; set; } = string.Empty;
        public int TodayCalories { get; set; }
        public List<Meal> FeaturedMeals { get; set; } = new();
        public List<MealWithRating> FeaturedMealsWithRatings { get; set; } = new();
        public List<(DateOnly Date, int Calories)> WeekCalories { get; set; } = new();
        public List<Order> RecentOrders { get; set; } = new();
    }
}
