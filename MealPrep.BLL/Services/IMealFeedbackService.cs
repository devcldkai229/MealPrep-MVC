using MealPrep.BLL.DTOs;
using MealPrep.DAL.Entities;

namespace MealPrep.BLL.Services
{
    /// <summary>
    /// 📊 Service xử lý Flow 8: Meal Feedback & Preference Learning
    /// 
    /// === RESPONSIBILITIES ===
    /// 1. Lấy danh sách món cần đánh giá (món của ngày hôm qua)
    /// 2. Submit rating từ User
    /// 3. Tự động chặn món nếu User request (1-2 sao)
    /// 4. Ghi vào NutritionLog khi confirm "Consumed"
    /// 5. Tạo report cho Admin (món bị chê nhiều)
    /// 6. Tích hợp vào thuật toán auto-assign meals (ưu tiên món 4-5 sao)
    /// </summary>
    public interface IMealFeedbackService
    {
        /// <summary>
        /// 🔍 Lấy danh sách món của ngày hôm qua (hoặc date cụ thể) chưa được đánh giá
        /// Dùng để hiển thị popup/banner "Hôm qua bạn ăn có ngon không?"
        /// </summary>
        Task<List<PendingFeedbackDto>> GetPendingFeedbacksAsync(Guid userId, DateOnly? date = null);

        /// <summary>
        /// 🔔 Kiểm tra User có món cần đánh giá không? (cho Dashboard notification)
        /// </summary>
        Task<FeedbackNotificationDto> CheckPendingFeedbackNotificationAsync(Guid userId);

        /// <summary>
        /// ⭐ Submit rating cho một món ăn
        /// 
        /// === LOGIC ===
        /// 1. Lưu MealRating vào DB
        /// 2. Nếu Stars = 1-2 và RequestBlock = true
        ///    → Tự động thêm vào UserDislikedMeal
        /// 3. Nếu MarkedAsConsumed = true
        ///    → Ghi vào NutritionLog
        /// 4. Return result
        /// </summary>
        Task<SubmitRatingResult> SubmitMealRatingAsync(Guid userId, SubmitMealRatingDto dto);

        /// <summary>
        /// 📊 Admin Report: Danh sách món đang bị đánh giá thấp
        /// Sắp xếp theo: LowRatingsCount DESC, AverageStars ASC
        /// </summary>
        Task<List<MealFeedbackReportDto>> GetLowRatedMealsReportAsync(int minRatings = 5);

        /// <summary>
        /// 📈 Lấy thống kê feedback của một User
        /// </summary>
        Task<UserFeedbackSummaryDto> GetUserFeedbackSummaryAsync(Guid userId);

        /// <summary>
        /// 🎯 Lấy danh sách món được User đánh giá cao (4-5 sao)
        /// </summary>
        Task<List<int>> GetUserPreferredMealIdsAsync(Guid userId);

        /// <summary>
        /// 📊 Lấy average rating của một món
        /// </summary>
        Task<decimal> GetMealAverageRatingAsync(int mealId);

        /// <summary>
        /// 📊 Lấy số lượng đánh giá của một món
        /// </summary>
        Task<int> GetMealRatingCountAsync(int mealId);

        /// <summary>
        /// 📋 Lấy danh sách tất cả ratings của một món (kèm thông tin user).
        /// </summary>
        Task<List<MealRating>> GetMealRatingsAsync(int mealId);
    }
}
