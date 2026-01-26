using MealPrep.BLL.Services;
using MealPrep.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSubscriptionsController : Controller
    {
        private readonly IAdminSubscriptionService _adminSubscriptionService;
        private readonly ILogger<AdminSubscriptionsController> _logger;

        public AdminSubscriptionsController(
            IAdminSubscriptionService adminSubscriptionService,
            ILogger<AdminSubscriptionsController> logger)
        {
            _adminSubscriptionService = adminSubscriptionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search, 
            SubscriptionStatus? status, 
            int page = 1, 
            int pageSize = 20)
        {
            var totalCount = await _adminSubscriptionService.GetSubscriptionsCountAsync(search, status);
            var subscriptions = await _adminSubscriptionService.GetSubscriptionsAsync(search, status, page, pageSize);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(subscriptions);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var subscription = await _adminSubscriptionService.GetSubscriptionDetailsAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            return View(subscription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, SubscriptionStatus status)
        {
            try
            {
                await _adminSubscriptionService.UpdateSubscriptionStatusAsync(id, status);
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái subscription #{id}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription status {SubscriptionId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật trạng thái: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _adminSubscriptionService.CancelSubscriptionAsync(id);
                TempData["SuccessMessage"] = $"Đã hủy subscription #{id}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi hủy subscription: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
