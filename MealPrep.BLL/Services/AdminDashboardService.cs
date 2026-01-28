using MealPrep.BLL.DTOs;
using MealPrep.DAL.Data;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AppDbContext _context;

        public AdminDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTodayRevenueAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Set<Payment>()
                .Where(p => p.Status == "Paid" && p.PaidAt.HasValue && p.PaidAt.Value.Date == today)
                .SumAsync(p => p.Amount);
        }

        public async Task<int> GetActiveSubscribersCountAsync()
        {
            return await _context.Set<Subscription>()
                .Where(s => s.Status == SubscriptionStatus.Active)
                .CountAsync();
        }

        public async Task<int> GetTomorrowOrdersCountAsync()
        {
            var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            return await _context.Set<DeliveryOrder>()
                .Where(d => d.DeliveryDate == tomorrow && d.Status == OrderStatus.Planned)
                .CountAsync();
        }

        public async Task<List<KitchenPrepForecastDto>> GetKitchenPrepForecastAsync(DateOnly? date = null)
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
            
            return await _context.Set<DeliveryOrderItem>()
                .Include(i => i.DeliveryOrder)
                .Include(i => i.Meal)
                .Where(i => i.DeliveryOrder != null && 
                           i.DeliveryOrder.DeliveryDate == targetDate &&
                           i.MealId.HasValue &&
                           i.Meal != null)
                .GroupBy(i => new { MealId = i.MealId!.Value, i.Meal!.Name })
                .Select(g => new KitchenPrepForecastDto
                {
                    MealId = g.Key.MealId,
                    MealName = g.Key.Name,
                    Quantity = g.Sum(i => i.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .ToListAsync();
        }

        public async Task<List<RevenueGrowthDto>> GetRevenueGrowthAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;
            
            return await _context.Set<Payment>()
                .Where(p => p.Status == "Paid" && p.PaidAt.HasValue && p.PaidAt.Value.Date >= startDate)
                .GroupBy(p => DateOnly.FromDateTime(p.PaidAt!.Value.Date))
                .Select(g => new RevenueGrowthDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(p => p.Amount)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<List<RevenueGrowthDto>> GetMonthlyRevenueAsync()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1).Date;
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).Date;
            
            // Get all days in current month
            var allDays = Enumerable.Range(1, DateTime.DaysInMonth(now.Year, now.Month))
                .Select(day => new DateOnly(now.Year, now.Month, day))
                .ToList();
            
            // Get revenue data
            var revenueData = await _context.Set<Payment>()
                .Where(p => p.Status == "Paid" && 
                           p.PaidAt.HasValue && 
                           p.PaidAt.Value.Date >= startOfMonth && 
                           p.PaidAt.Value.Date <= endOfMonth)
                .GroupBy(p => DateOnly.FromDateTime(p.PaidAt!.Value.Date))
                .Select(g => new RevenueGrowthDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(p => p.Amount)
                })
                .ToListAsync();
            
            // Create map for quick lookup
            var revenueMap = revenueData.ToDictionary(r => r.Date, r => r.Revenue);
            
            // Return all days with revenue (0 if no revenue)
            return allDays.Select(date => new RevenueGrowthDto
            {
                Date = date,
                Revenue = revenueMap.ContainsKey(date) ? revenueMap[date] : 0
            }).ToList();
        }

        public async Task<List<RevenueGrowthDto>> GetRevenueByMonthAsync(int year, int month)
        {
            var startOfMonth = new DateTime(year, month, 1).Date;
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1).Date;
            
            // Get all days in the specified month
            var allDays = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
                .Select(day => new DateOnly(year, month, day))
                .ToList();
            
            // Get revenue data
            var revenueData = await _context.Set<Payment>()
                .Where(p => p.Status == "Paid" && 
                           p.PaidAt.HasValue && 
                           p.PaidAt.Value.Date >= startOfMonth && 
                           p.PaidAt.Value.Date <= endOfMonth)
                .GroupBy(p => DateOnly.FromDateTime(p.PaidAt!.Value.Date))
                .Select(g => new RevenueGrowthDto
                {
                    Date = g.Key,
                    Revenue = g.Sum(p => p.Amount)
                })
                .ToListAsync();
            
            // Create map for quick lookup
            var revenueMap = revenueData.ToDictionary(r => r.Date, r => r.Revenue);
            
            // Return all days with revenue (0 if no revenue)
            return allDays.Select(date => new RevenueGrowthDto
            {
                Date = date,
                Revenue = revenueMap.ContainsKey(date) ? revenueMap[date] : 0
            }).ToList();
        }

        public async Task<Dictionary<string, int>> GetOrdersStatusDistributionAsync()
        {
            return await _context.Set<DeliveryOrder>()
                .GroupBy(o => o.Status.ToString())
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }


        public async Task<List<SubscriptionGrowthDto>> GetSubscriptionGrowthAsync(int days = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;
            
            return await _context.Set<Subscription>()
                .Where(s => s.CreatedAt.Date >= startDate)
                .GroupBy(s => DateOnly.FromDateTime(s.CreatedAt.Date))
                .Select(g => new SubscriptionGrowthDto
                {
                    Date = g.Key,
                    NewSubscriptions = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetUserSegmentationByGoalAsync()
        {
            return await _context.Set<UserNutritionProfile>()
                .Where(p => p.Goal != null)
                .GroupBy(p => p.Goal!.ToString())
                .Select(g => new { Goal = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Goal, x => x.Count);
        }

        public async Task<List<TopDislikedMealDto>> GetTopDislikedMealsAsync(int top = 5)
        {
            return await _context.Set<UserDislikedMeal>()
                .Include(d => d.Meal)
                .Where(d => d.Meal != null)
                .GroupBy(d => new { d.MealId, d.Meal!.Name })
                .Select(g => new TopDislikedMealDto
                {
                    MealId = g.Key.MealId,
                    MealName = g.Key.Name,
                    DislikeCount = g.Count()
                })
                .OrderByDescending(x => x.DislikeCount)
                .Take(top)
                .ToListAsync();
        }

        public async Task<List<AtRiskSubscriptionDto>> GetAtRiskSubscriptionsAsync(int daysAhead = 3)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var endDate = today.AddDays(daysAhead);
            
            return await _context.Set<Subscription>()
                .Where(s => s.Status == SubscriptionStatus.Active &&
                           s.EndDate.HasValue &&
                           s.EndDate.Value >= today &&
                           s.EndDate.Value <= endDate)
                .Select(s => new AtRiskSubscriptionDto
                {
                    SubscriptionId = s.Id,
                    CustomerName = s.CustomerName,
                    CustomerEmail = s.CustomerEmail,
                    EndDate = s.EndDate,
                    DaysUntilExpiry = s.EndDate!.Value.DayNumber - today.DayNumber,
                    TotalAmount = s.TotalAmount
                })
                .OrderBy(s => s.EndDate)
                .ToListAsync();
        }

        public async Task<List<FailedPaymentDto>> GetFailedPaymentsAsync(int top = 10)
        {
            return await _context.Set<Payment>()
                .Include(p => p.AppUser)
                .Include(p => p.Subscription)
                .Where(p => p.Status == "Failed")
                .OrderByDescending(p => p.CreatedAt)
                .Take(top)
                .Select(p => new FailedPaymentDto
                {
                    PaymentId = p.Id,
                    PaymentCode = p.PaymentCode,
                    CustomerName = p.AppUser != null ? p.AppUser.FullName : p.Subscription != null ? p.Subscription.CustomerName : "N/A",
                    CustomerEmail = p.AppUser != null ? p.AppUser.Email : p.Subscription != null ? p.Subscription.CustomerEmail : "N/A",
                    Amount = p.Amount,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<TopAllergyDto>> GetTopAllergiesAsync()
        {
            return await _context.Set<UserAllergy>()
                .GroupBy(a => a.AllergyName)
                .Select(g => new TopAllergyDto
                {
                    AllergyName = g.Key,
                    UserCount = g.Count()
                })
                .OrderByDescending(x => x.UserCount)
                .ToListAsync();
        }
    }
}
