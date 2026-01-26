using MealPrep.BLL.DTOs;
using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    /// <summary>
    /// 📊 Controller xử lý Flow 8: Meal Feedback & Preference Learning
    /// </summary>
    [Authorize]
    public class MealFeedbackController : Controller
    {
        private readonly IMealFeedbackService _feedbackService;
        private readonly ILogger<MealFeedbackController> _logger;

        public MealFeedbackController(
            IMealFeedbackService feedbackService,
            ILogger<MealFeedbackController> logger)
        {
            _feedbackService = feedbackService;
            _logger = logger;
        }

        /// <summary>
        /// 📋 GET: Trang đánh giá món ăn (hiển thị món của ngày hôm qua)
        /// Route: /MealFeedback
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(DateOnly? date = null)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var targetDate = date ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

            var pendingFeedbacks = await _feedbackService.GetPendingFeedbacksAsync(userId, targetDate);

            ViewBag.TargetDate = targetDate;
            ViewBag.IsYesterday = targetDate == DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

            return View(pendingFeedbacks);
        }

        /// <summary>
        /// ⭐ POST: Submit rating cho một món ăn
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRating(
            int deliveryOrderItemId,
            int mealId,
            DateOnly deliveryDate,
            int stars,
            string? tags,
            string? comments)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                // Parse tags từ comma-separated string
                var tagsList = string.IsNullOrWhiteSpace(tags)
                    ? null
                    : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList();

                var dto = new SubmitMealRatingDto(
                    deliveryOrderItemId,
                    mealId,
                    deliveryDate,
                    stars,
                    tagsList,
                    comments
                );

                var result = await _feedbackService.SubmitMealRatingAsync(userId, dto);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = result.Message;

                    if (result.AddedToNutritionLog)
                    {
                        TempData["InfoMessage"] = "📊 Đã ghi nhận vào nhật ký dinh dưỡng của bạn.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }

                return RedirectToAction(nameof(Index), new { date = deliveryDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting rating");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi ghi nhận đánh giá. Vui lòng thử lại.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// 📊 GET: Trang thống kê feedback của User
        /// Route: /MealFeedback/MySummary
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MySummary()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var summary = await _feedbackService.GetUserFeedbackSummaryAsync(userId);

            return View(summary);
        }

        /// <summary>
        /// 📊 GET: Admin Report - Món bị đánh giá thấp
        /// Route: /MealFeedback/AdminReport
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminReport(int minRatings = 5)
        {
            var report = await _feedbackService.GetLowRatedMealsReportAsync(minRatings);

            ViewBag.MinRatings = minRatings;

            return View(report);
        }

        /// <summary>
        /// 🔔 API: Check pending feedback notification (dùng cho Dashboard)
        /// Route: /MealFeedback/CheckNotification
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckNotification()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var notification = await _feedbackService.CheckPendingFeedbackNotificationAsync(userId);

            return Json(notification);
        }
    }
}
