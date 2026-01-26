using MealPrep.BLL.DTOs;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using MealPrep.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepository<Subscription> _subs;
        private readonly IRepository<DeliveryOrder> _deliveryOrders;
        private readonly IRepository<NutritionLog> _logs;
        private readonly IRepository<Meal> _meals;

        public DashboardService(
            IRepository<Subscription> subs,
            IRepository<DeliveryOrder> deliveryOrders,
            IRepository<NutritionLog> logs,
            IRepository<Meal> meals)
        {
            _subs = subs;
            _deliveryOrders = deliveryOrders;
            _logs = logs;
            _meals = meals;
        }

        public async Task<DashboardDto> GetDashboardDataAsync(Guid userId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var activeSub = await _subs.Query()
                .Where(s => s.AppUserId == userId && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();
            
            // Fallback to latest subscription if no active one
            if (activeSub == null)
            {
                activeSub = await _subs.Query()
                    .Where(s => s.AppUserId == userId)
                    .OrderByDescending(s => s.Id)
                    .FirstOrDefaultAsync();
            }

            // Get next delivery order through subscription
            DeliveryOrder? nextOrder = null;
            if (activeSub != null)
            {
                nextOrder = await _deliveryOrders.Query()
                    .Include(o => o.DeliverySlot)
                    .Include(o => o.Subscription)
                    .Where(o => o.SubscriptionId == activeSub.Id && 
                               o.DeliveryDate >= today &&
                               o.Subscription!.AppUserId == userId)
                    .OrderBy(o => o.DeliveryDate)
                    .FirstOrDefaultAsync();
            }

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

            // Get recent delivery orders through subscriptions
            var userSubscriptionIds = await _subs.Query()
                .Where(s => s.AppUserId == userId)
                .Select(s => s.Id)
                .ToListAsync();

            var recentDeliveryOrders = await _deliveryOrders.Query()
                .Include(o => o.DeliverySlot)
                .Include(o => o.Items).ThenInclude(i => i.Meal)
                .Where(o => userSubscriptionIds.Contains(o.SubscriptionId))
                .OrderByDescending(o => o.DeliveryDate)
                .Take(5)
                .ToListAsync();

            // Map DeliveryOrder to Order for backward compatibility with DTO
            // Note: DashboardDto.RecentOrders expects List<Order>, but we're using DeliveryOrder
            // We need to check the DTO structure
            var recentOrders = recentDeliveryOrders.Select(deliveryOrder => new Order
            {
                Id = deliveryOrder.Id,
                SubscriptionId = deliveryOrder.SubscriptionId,
                DeliveryDate = deliveryOrder.DeliveryDate,
                DeliverySlotId = deliveryOrder.DeliverySlotId ?? 0,
                DeliverySlot = deliveryOrder.DeliverySlot,
                Status = deliveryOrder.Status,
                Items = deliveryOrder.Items.Select(deliveryItem => new OrderItem
                {
                    Id = deliveryItem.Id,
                    OrderId = deliveryOrder.Id,
                    MealId = deliveryItem.MealId ?? 0,
                    Meal = deliveryItem.Meal,
                    Quantity = deliveryItem.Quantity
                }).ToList()
            }).ToList();

            return new DashboardDto
            {
                SubscriptionStatus = activeSub?.Status.ToString() ?? "No subscription",
                NextDeliveryText = nextOrder == null ? "No upcoming delivery" : $"{nextOrder.DeliveryDate} • {nextOrder.DeliverySlot?.Name}",
                TodayCalories = todayCalories,
                FeaturedMeals = featured,
                WeekCalories = week,
                RecentOrders = recentOrders
            };
        }
    }
}
