namespace MealPrep.BLL.DTOs
{
    /// <summary>
    /// 📋 Danh sách món cần đánh giá (món của ngày hôm qua chưa rate)
    /// </summary>
    public record PendingFeedbackDto(
        int DeliveryOrderItemId,
        int MealId,
        string MealName,
        DateOnly DeliveryDate,
        int Calories,
        decimal Protein,
        decimal Carbs,
        decimal Fat,
        string? MealImageUrl
    );

    /// <summary>
    /// 📝 Submit rating từ User
    /// </summary>
    public record SubmitMealRatingDto(
        int DeliveryOrderItemId,
        int MealId,
        DateOnly DeliveryDate,
        int Stars,
        List<string>? Tags,
        string? Comments
    );

    /// <summary>
    /// 📊 Kết quả submit rating
    /// </summary>
    public record SubmitRatingResult(
        bool Success,
        string Message,
        bool AddedToNutritionLog // Đã ghi vào NutritionLog chưa?
    );

    /// <summary>
    /// 📈 Admin Report: Món đang bị chê nhiều
    /// </summary>
    public record MealFeedbackReportDto(
        int MealId,
        string MealName,
        int TotalRatings,
        decimal AverageStars,
        int LowRatingsCount, // 1-2 sao
        int HighRatingsCount, // 4-5 sao
        List<string> CommonNegativeTags, // Tags hay gặp trong low ratings
        int BlockRequestsCount // Số lần bị request block
    );

    /// <summary>
    /// 📊 Feedback summary cho một User
    /// </summary>
    public record UserFeedbackSummaryDto(
        Guid UserId,
        int TotalRatings,
        int TotalMealsConsumed,
        decimal AverageStars,
        int DislikedMealsCount,
        List<string> FavoriteTags
    );

    /// <summary>
    /// 🔔 Notification DTO để hiện popup/banner
    /// </summary>
    public record FeedbackNotificationDto(
        bool HasPendingFeedback,
        int PendingCount,
        DateOnly FeedbackDate,
        string Message
    );
}
