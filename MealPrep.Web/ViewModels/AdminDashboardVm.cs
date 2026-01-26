using MealPrep.BLL.DTOs;
using System.Collections.Generic;

namespace MealPrep.Web.ViewModels
{
    public class AdminDashboardVm
    {
        // Quick Stats
        public decimal TodayRevenue { get; set; }
        public int ActiveSubscribers { get; set; }
        public int TomorrowOrders { get; set; }
        
        // Charts Data
        public List<KitchenPrepForecastDto> KitchenPrepForecast { get; set; } = new();
        public List<RevenueGrowthDto> RevenueGrowth { get; set; } = new();
        public List<RevenueGrowthDto> MonthlyRevenue { get; set; } = new();
        public List<SubscriptionGrowthDto> SubscriptionGrowth { get; set; } = new();
        public Dictionary<string, int> UserSegmentationByGoal { get; set; } = new();
        public List<TopDislikedMealDto> TopDislikedMeals { get; set; } = new();
        public Dictionary<string, int> OrdersStatusDistribution { get; set; } = new();
        
        // Data Tables
        public List<AtRiskSubscriptionDto> AtRiskSubscriptions { get; set; } = new();
        public List<FailedPaymentDto> FailedPayments { get; set; } = new();
        public List<TopAllergyDto> TopAllergies { get; set; } = new();
    }
}
