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
        /// 📋 GET: Trang đánh giá món ăn
        /// Route: /MealFeedback
        /// Route: /MealFeedback?date=2024-01-27 (filter theo ngày cụ thể)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(DateOnly? date = null)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            _logger.LogInformation("🔍 MealFeedback Index - UserId: {UserId}, Date filter: {Date}", 
                userId, date?.ToString() ?? "ALL");

            // ✅ THAY ĐỔI: Không pass date nếu user không chọn ngày cụ thể
            // → Sẽ hiển thị TẤT CẢ món chưa đánh giá
            var pendingFeedbacks = await _feedbackService.GetPendingFeedbacksAsync(userId, date);

            // Filter món trong 7 ngày gần đây nếu không có date filter
            if (!date.HasValue)
            {
                var recent7Days = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));
                pendingFeedbacks = pendingFeedbacks
                    .Where(f => f.DeliveryDate >= recent7Days && f.DeliveryDate <= DateOnly.FromDateTime(DateTime.Today))
                    .OrderByDescending(f => f.DeliveryDate)
                    .ToList();
                    
                _logger.LogInformation("📊 Filtered to last 7 days: {Count} items", pendingFeedbacks.Count);
            }

            ViewBag.TargetDate = date;
            ViewBag.IsFiltered = date.HasValue;
            ViewBag.TotalPendingCount = pendingFeedbacks.Count;

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

                // ✅ Redirect về Index không có date filter để hiển thị tất cả món chưa đánh giá
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> AdminReport(int minRatings = 5, int? starFilter = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var allReport = await _feedbackService.GetLowRatedMealsReportAsync(minRatings);

                // ✅ Filter theo số sao nếu có
                if (starFilter.HasValue && starFilter.Value >= 1 && starFilter.Value <= 5)
                {
                    allReport = allReport
                        .Where(m => Math.Floor(m.AverageStars) == starFilter.Value - 1 || 
                                    Math.Ceiling(m.AverageStars) == starFilter.Value)
                        .ToList();
                }

                // ✅ Phân trang
                var totalItems = allReport.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

                var pagedReport = allReport
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.MinRatings = minRatings;
                ViewBag.StarFilter = starFilter;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.PageSize = pageSize;

                return View(pagedReport);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin report");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải báo cáo.";
                return View(new List<MealFeedbackReportDto>());
            }
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
