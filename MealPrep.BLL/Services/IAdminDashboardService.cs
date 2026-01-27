using MealPrep.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IAdminDashboardService
    {
        // Quick Stats
        Task<decimal> GetTodayRevenueAsync();
        Task<int> GetActiveSubscribersCountAsync();
        Task<int> GetTomorrowOrdersCountAsync();
        
        // Charts Data
        Task<List<KitchenPrepForecastDto>> GetKitchenPrepForecastAsync(DateOnly? date = null);
        Task<List<RevenueGrowthDto>> GetRevenueGrowthAsync(int days = 30);
        Task<List<RevenueGrowthDto>> GetMonthlyRevenueAsync(); // Doanh thu theo ngày trong tháng hiện tại
        Task<List<RevenueGrowthDto>> GetRevenueByMonthAsync(int year, int month); // Doanh thu theo ngày trong tháng/năm cụ thể
        Task<List<SubscriptionGrowthDto>> GetSubscriptionGrowthAsync(int days = 30);
        Task<Dictionary<string, int>> GetUserSegmentationByGoalAsync();
        Task<List<TopDislikedMealDto>> GetTopDislikedMealsAsync(int top = 5);
        Task<Dictionary<string, int>> GetOrdersStatusDistributionAsync(); // Phân bố trạng thái đơn hàng
        
        // Data Tables
        Task<List<AtRiskSubscriptionDto>> GetAtRiskSubscriptionsAsync(int daysAhead = 3);
        Task<List<FailedPaymentDto>> GetFailedPaymentsAsync(int top = 10);
        Task<List<TopAllergyDto>> GetTopAllergiesAsync();
    }
}
