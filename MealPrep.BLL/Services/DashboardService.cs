using MealPrep.BLL.DTOs;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
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
        private readonly IRepository<UserNutritionProfile> _nutritionProfiles;
        private readonly IMealFeedbackService _feedbackService;

        public DashboardService(
            IRepository<Subscription> subs,
            IRepository<DeliveryOrder> deliveryOrders,
            IRepository<NutritionLog> logs,
            IRepository<Meal> meals,
            IRepository<UserNutritionProfile> nutritionProfiles,
            IMealFeedbackService feedbackService)
        {
            _subs = subs;
            _deliveryOrders = deliveryOrders;
            _logs = logs;
            _meals = meals;
            _nutritionProfiles = nutritionProfiles;
            _feedbackService = feedbackService;
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
                    .Include(o => o.Items)!.ThenInclude(i => i.DeliverySlot)
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

            // Get average ratings and rating counts for featured meals
            var featuredMealsWithRatings = new List<MealWithRating>();
            foreach (var meal in featured)
            {
                var avgRating = await _feedbackService.GetMealAverageRatingAsync(meal.Id);
                var ratingCount = await _feedbackService.GetMealRatingCountAsync(meal.Id);
                featuredMealsWithRatings.Add(new MealWithRating
                {
                    Meal = meal,
                    AverageRating = avgRating,
                    RatingCount = ratingCount
                });
            }

            // Week summary: last 7 days calories (including days with 0 calories)
            var from = today.AddDays(-6);
            var weekLogs = await _logs.Query()
                .Include(l => l.Meal)
                .Where(l => l.AppUserId == userId && l.Date >= from && l.Date <= today)
                .ToListAsync();

            // Group by date and calculate total calories
            var caloriesByDate = weekLogs
                .GroupBy(x => x.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Meal!.Calories * x.Quantity));

            // Ensure all 7 days are included (fill missing days with 0)
            var week = new List<(DateOnly Date, int Calories)>();
            for (var date = from; date <= today; date = date.AddDays(1))
            {
                week.Add((date, caloriesByDate.GetValueOrDefault(date, 0)));
            }

            // Get recent delivery orders through subscriptions
            var userSubscriptionIds = await _subs.Query()
                .Where(s => s.AppUserId == userId)
                .Select(s => s.Id)
                .ToListAsync();

            var recentDeliveryOrders = await _deliveryOrders.Query()
                .Include(o => o.Items)!.ThenInclude(i => i.Meal)
                .Include(o => o.Items)!.ThenInclude(i => i.DeliverySlot)
                .Where(o => userSubscriptionIds.Contains(o.SubscriptionId))
                .OrderByDescending(o => o.DeliveryDate)
                .Take(5)
                .ToListAsync();

            // Map DeliveryOrder to Order for backward compatibility with DTO
            // Note: DeliverySlot is now at item level, so we get first item's slot or null
            var recentOrders = recentDeliveryOrders.Select(deliveryOrder => new Order
            {
                Id = deliveryOrder.Id,
                SubscriptionId = deliveryOrder.SubscriptionId,
                DeliveryDate = deliveryOrder.DeliveryDate,
                DeliverySlotId = deliveryOrder.Items.FirstOrDefault()?.DeliverySlotId ?? 0,
                DeliverySlot = deliveryOrder.Items.FirstOrDefault()?.DeliverySlot, // Get slot from first item
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

            // Get user's nutrition profile to get CaloriesInDay for chart max value
            var nutritionProfile = await _nutritionProfiles.Query()
                .FirstOrDefaultAsync(np => np.AppUserId == userId);
            
            // Use CaloriesInDay from profile, fallback to 2000 if null
            var maxCalories = nutritionProfile?.CaloriesInDay ?? 2000;

            return new DashboardDto
            {
                SubscriptionStatus = activeSub?.Status.ToString() ?? "No subscription",
                NextDeliveryText = nextOrder == null ? "No upcoming delivery" : $"{nextOrder.DeliveryDate} • {nextOrder.Items.FirstOrDefault()?.DeliverySlot?.Name ?? "N/A"}",
                TodayCalories = todayCalories,
                FeaturedMeals = featured,
                FeaturedMealsWithRatings = featuredMealsWithRatings,
                WeekCalories = week,
                RecentOrders = recentOrders,
                MaxCalories = maxCalories
            };
        }
    }

    // Helper class to store meal with its rating
    public class MealWithRating
    {
        public Meal Meal { get; set; } = null!;
        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; } = 0; // Total number of ratings
    }
}
