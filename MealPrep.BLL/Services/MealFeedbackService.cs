using MealPrep.BLL.DTOs;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MealPrep.BLL.Services
{
    public class MealFeedbackService : IMealFeedbackService
    {
        private readonly IRepository<MealRating> _ratingRepo;
        private readonly IRepository<DeliveryOrderItem> _deliveryItemRepo;
        private readonly IRepository<DeliveryOrder> _deliveryOrderRepo;
        private readonly IRepository<UserDislikedMeal> _dislikedMealRepo;
        private readonly IRepository<NutritionLog> _nutritionLogRepo;
        private readonly IRepository<Meal> _mealRepo;
        private readonly ILogger<MealFeedbackService> _logger;

        public MealFeedbackService(
            IRepository<MealRating> ratingRepo,
            IRepository<DeliveryOrderItem> deliveryItemRepo,
            IRepository<DeliveryOrder> deliveryOrderRepo,
            IRepository<UserDislikedMeal> dislikedMealRepo,
            IRepository<NutritionLog> nutritionLogRepo,
            IRepository<Meal> mealRepo,
            ILogger<MealFeedbackService> logger)
        {
            _ratingRepo = ratingRepo;
            _deliveryItemRepo = deliveryItemRepo;
            _deliveryOrderRepo = deliveryOrderRepo;
            _dislikedMealRepo = dislikedMealRepo;
            _nutritionLogRepo = nutritionLogRepo;
            _mealRepo = mealRepo;
            _logger = logger;
        }

        /// <summary>
        /// 🔍 Lấy danh sách món của ngày hôm qua (hoặc date cụ thể) chưa được đánh giá
        /// 
        /// === WORKFLOW ===
        /// 1. Lấy ngày cần query (default = hôm qua)
        /// 2. Query DeliveryOrders của User có DeliveryDate = targetDate
        /// 3. Flatten DeliveryOrderItems
        /// 4. Filter: Chưa có rating (LEFT JOIN MealRating)
        /// 5. Map sang PendingFeedbackDto
        /// </summary>
        public async Task<List<PendingFeedbackDto>> GetPendingFeedbacksAsync(Guid userId, DateOnly? date = null)
        {
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

            _logger.LogInformation("📋 Getting pending feedbacks for User {UserId} on {Date}", userId, targetDate);

            try
            {
                // Query: Lấy DeliveryOrders của User cho ngày targetDate
                var deliveryOrders = await _deliveryOrderRepo.Query()
                    .Include(d => d.Items)
                        .ThenInclude(i => i.Meal)
                    .Include(d => d.Subscription)
                    .Where(d =>
                        d.Subscription!.AppUserId == userId &&
                        d.DeliveryDate == targetDate &&
                        d.Status == DAL.Enums.OrderStatus.Delivered) // Chỉ lấy đơn đã giao
                    .ToListAsync();

                if (!deliveryOrders.Any())
                {
                    _logger.LogInformation("📭 No delivered orders found for {Date}", targetDate);
                    return new List<PendingFeedbackDto>();
                }

                // Flatten items và filter chưa có rating
                var ratedItemIds = await _ratingRepo.Query()
                    .Where(r => r.AppUserId == userId && r.DeliveryDate == targetDate)
                    .Select(r => r.DeliveryOrderItemId)
                    .ToListAsync();

                var pendingItems = deliveryOrders
                    .SelectMany(d => d.Items)
                    .Where(i => i.MealId.HasValue && !ratedItemIds.Contains(i.Id))
                    .ToList();

                var result = pendingItems.Select(item => new PendingFeedbackDto(
                    item.Id,
                    item.MealId!.Value,
                    item.MealNameSnapshot,
                    targetDate,
                    item.Meal?.Calories ?? 0,
                    item.Meal?.Protein ?? 0,
                    item.Meal?.Carbs ?? 0,
                    item.Meal?.Fat ?? 0,
                    GetFirstMealImage(item.Meal?.Images)
                )).ToList();

                _logger.LogInformation("✅ Found {Count} pending feedbacks", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting pending feedbacks");
                throw;
            }
        }

        /// <summary>
        /// 🔔 Kiểm tra User có món cần đánh giá không?
        /// </summary>
        public async Task<FeedbackNotificationDto> CheckPendingFeedbackNotificationAsync(Guid userId)
        {
            var yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var pendingFeedbacks = await GetPendingFeedbacksAsync(userId, yesterday);

            if (pendingFeedbacks.Any())
            {
                return new FeedbackNotificationDto(
                    true,
                    pendingFeedbacks.Count,
                    yesterday,
                    $"Hôm qua bạn ăn có ngon không? Đánh giá {pendingFeedbacks.Count} món để giúp chúng tôi cải thiện!"
                );
            }

            return new FeedbackNotificationDto(false, 0, yesterday, string.Empty);
        }

        /// <summary>
        /// ⭐ Submit rating cho một món ăn
        /// 
        /// === WORKFLOW ===
        /// 1. Validate: DeliveryOrderItem có thuộc về User không?
        /// 2. Lưu MealRating vào DB
        /// 3. Logic xử lý theo Stars:
        ///    a. Nếu 1-2 sao + RequestBlock = true
        ///       → Thêm vào UserDislikedMeal
        ///    b. Nếu 4-5 sao
        ///       → (Optional) Tăng priority score (hiện tại skip)
        /// 4. Ghi vào NutritionLog (confirm đã ăn)
        /// 5. Return result
        /// </summary>
        public async Task<SubmitRatingResult> SubmitMealRatingAsync(Guid userId, SubmitMealRatingDto dto)
        {
            _logger.LogInformation("⭐ User {UserId} submitting rating for DeliveryOrderItem {ItemId}", 
                userId, dto.DeliveryOrderItemId);

            try
            {
                // === STEP 1: Validate ownership ===
                var deliveryItem = await _deliveryItemRepo.Query()
                    .Include(i => i.DeliveryOrder)
                        .ThenInclude(d => d!.Subscription)
                    .FirstOrDefaultAsync(i => i.Id == dto.DeliveryOrderItemId);

                if (deliveryItem == null)
                {
                    _logger.LogWarning("❌ DeliveryOrderItem {ItemId} not found", dto.DeliveryOrderItemId);
                    return new SubmitRatingResult(false, "Món ăn không tồn tại", false, false);
                }

                if (deliveryItem.DeliveryOrder?.Subscription?.AppUserId != userId)
                {
                    _logger.LogWarning("❌ User {UserId} does not own DeliveryOrderItem {ItemId}", 
                        userId, dto.DeliveryOrderItemId);
                    return new SubmitRatingResult(false, "Bạn không có quyền đánh giá món này", false, false);
                }

                // === STEP 2: Check duplicate rating ===
                var existingRating = await _ratingRepo.Query()
                    .FirstOrDefaultAsync(r => r.AppUserId == userId && r.DeliveryOrderItemId == dto.DeliveryOrderItemId);

                if (existingRating != null)
                {
                    _logger.LogInformation("🔄 Updating existing rating {RatingId}", existingRating.Id);
                    
                    // Update existing rating
                    existingRating.Stars = dto.Stars;
                    existingRating.Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null;
                    existingRating.Comments = dto.Comments;
                    existingRating.RequestedBlock = dto.RequestBlock;
                    existingRating.UpdatedAt = DateTime.UtcNow;

                    _ratingRepo.Update(existingRating);
                }
                else
                {
                    // === STEP 3: Create new rating ===
                    var rating = new MealRating
                    {
                        AppUserId = userId,
                        DeliveryOrderItemId = dto.DeliveryOrderItemId,
                        MealId = dto.MealId,
                        DeliveryDate = dto.DeliveryDate,
                        Stars = dto.Stars,
                        Tags = dto.Tags != null ? JsonSerializer.Serialize(dto.Tags) : null,
                        Comments = dto.Comments,
                        RequestedBlock = dto.RequestBlock,
                        MarkedAsConsumed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _ratingRepo.AddAsync(rating);
                }

                await _ratingRepo.SaveChangesAsync();

                // === STEP 4: Handle low ratings (1-2 sao) ===
                bool blockedMeal = false;
                if (dto.Stars <= 2 && dto.RequestBlock)
                {
                    var alreadyDisliked = await _dislikedMealRepo.Query()
                        .AnyAsync(d => d.AppUserId == userId && d.MealId == dto.MealId);

                    if (!alreadyDisliked)
                    {
                        var dislikedMeal = new UserDislikedMeal
                        {
                            AppUserId = userId,
                            MealId = dto.MealId
                        };

                        await _dislikedMealRepo.AddAsync(dislikedMeal);
                        await _dislikedMealRepo.SaveChangesAsync();

                        blockedMeal = true;
                        _logger.LogInformation("🚫 User {UserId} blocked Meal {MealId}", userId, dto.MealId);
                    }
                }

                // === STEP 5: Ghi vào NutritionLog ===
                bool addedToNutritionLog = false;
                var existingLog = await _nutritionLogRepo.Query()
                    .FirstOrDefaultAsync(n =>
                        n.AppUserId == userId &&
                        n.MealId == dto.MealId &&
                        n.Date == dto.DeliveryDate);

                if (existingLog == null)
                {
                    var user = await _deliveryItemRepo.Query()
                        .Where(i => i.Id == dto.DeliveryOrderItemId)
                        .Select(i => i.DeliveryOrder!.Subscription!.AppUser)
                        .FirstOrDefaultAsync();

                    if (user != null)
                    {
                        var nutritionLog = new NutritionLog
                        {
                            AppUserId = userId,
                            CustomerEmail = user.Email,
                            Date = dto.DeliveryDate,
                            MealId = dto.MealId,
                            Quantity = deliveryItem.Quantity
                        };

                        await _nutritionLogRepo.AddAsync(nutritionLog);
                        await _nutritionLogRepo.SaveChangesAsync();

                        addedToNutritionLog = true;
                        _logger.LogInformation("📊 Added to NutritionLog for User {UserId}", userId);
                    }
                }

                var message = dto.Stars <= 2 
                    ? "Cảm ơn phản hồi! Chúng tôi sẽ cải thiện món này."
                    : "Cảm ơn! Rất vui vì bạn thích món này! 🎉";

                return new SubmitRatingResult(true, message, blockedMeal, addedToNutritionLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error submitting rating");
                return new SubmitRatingResult(false, "Đã xảy ra lỗi khi ghi nhận đánh giá", false, false);
            }
        }

        /// <summary>
        /// 📊 Admin Report: Danh sách món đang bị đánh giá thấp
        /// </summary>
        public async Task<List<MealFeedbackReportDto>> GetLowRatedMealsReportAsync(int minRatings = 5)
        {
            _logger.LogInformation("📊 Generating low-rated meals report");

            try
            {
                var allRatings = await _ratingRepo.Query()
                    .Include(r => r.Meal)
                    .Where(r => r.Meal != null)
                    .ToListAsync();

                var reportData = allRatings
                    .GroupBy(r => new { r.MealId, r.Meal!.Name })
                    .Select(g => new
                    {
                        g.Key.MealId,
                        g.Key.Name,
                        TotalRatings = g.Count(),
                        AverageStars = g.Average(r => r.Stars),
                        LowRatings = g.Count(r => r.Stars <= 2),
                        HighRatings = g.Count(r => r.Stars >= 4),
                        BlockRequests = g.Count(r => r.RequestedBlock),
                        Tags = g.Where(r => r.Stars <= 2 && !string.IsNullOrEmpty(r.Tags))
                                .SelectMany(r => ParseTags(r.Tags))
                                .GroupBy(t => t)
                                .OrderByDescending(t => t.Count())
                                .Take(5)
                                .Select(t => $"{t.Key} ({t.Count()})")
                                .ToList()
                    })
                    .Where(x => x.TotalRatings >= minRatings)
                    .OrderByDescending(x => x.LowRatings)
                    .ThenBy(x => x.AverageStars)
                    .Take(20)
                    .ToList();

                var result = reportData.Select(x => new MealFeedbackReportDto(
                    x.MealId,
                    x.Name,
                    x.TotalRatings,
                    (decimal)Math.Round(x.AverageStars, 2),
                    x.LowRatings,
                    x.HighRatings,
                    x.Tags,
                    x.BlockRequests
                )).ToList();

                _logger.LogInformation("✅ Generated report with {Count} meals", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating report");
                throw;
            }
        }

        /// <summary>
        /// 📈 Lấy thống kê feedback của một User
        /// </summary>
        public async Task<UserFeedbackSummaryDto> GetUserFeedbackSummaryAsync(Guid userId)
        {
            var ratings = await _ratingRepo.Query()
                .Where(r => r.AppUserId == userId)
                .ToListAsync();

            var dislikedCount = await _dislikedMealRepo.Query()
                .CountAsync(d => d.AppUserId == userId);

            var favoriteTags = ratings
                .Where(r => r.Stars >= 4 && !string.IsNullOrEmpty(r.Tags))
                .SelectMany(r => ParseTags(r.Tags))
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            return new UserFeedbackSummaryDto(
                userId,
                ratings.Count,
                ratings.Count(r => r.MarkedAsConsumed),
                ratings.Any() ? (decimal)Math.Round(ratings.Average(r => r.Stars), 2) : 0,
                dislikedCount,
                favoriteTags
            );
        }

        /// <summary>
        /// 🎯 Lấy danh sách món được User đánh giá cao (4-5 sao)
        /// </summary>
        public async Task<List<int>> GetUserPreferredMealIdsAsync(Guid userId)
        {
            return await _ratingRepo.Query()
                .Where(r => r.AppUserId == userId && r.Stars >= 4)
                .Select(r => r.MealId)
                .Distinct()
                .ToListAsync();
        }

        /// <summary>
        /// 📊 Lấy average rating của một món
        /// </summary>
        public async Task<decimal> GetMealAverageRatingAsync(int mealId)
        {
            var ratings = await _ratingRepo.Query()
                .Where(r => r.MealId == mealId)
                .Select(r => r.Stars)
                .ToListAsync();

            return ratings.Any() ? (decimal)ratings.Average() : 0;
        }

        // === HELPER METHODS ===

        private string? GetFirstMealImage(string? imagesJson)
        {
            if (string.IsNullOrEmpty(imagesJson)) return null;

            try
            {
                var images = JsonSerializer.Deserialize<List<string>>(imagesJson);
                return images?.FirstOrDefault();
            }
            catch
            {
                return imagesJson.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
            }
        }

        private List<string> ParseTags(string? tagsJson)
        {
            if (string.IsNullOrEmpty(tagsJson)) return new List<string>();

            try
            {
                return JsonSerializer.Deserialize<List<string>>(tagsJson) ?? new List<string>();
            }
            catch
            {
                return tagsJson.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToList();
            }
        }
    }
}
