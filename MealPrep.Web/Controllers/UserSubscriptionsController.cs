using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    [Authorize]
    public class UserSubscriptionsController : Controller
    {
        private readonly IUserSubscriptionService _userSubscriptionService;
        private readonly ILogger<UserSubscriptionsController> _logger;

        public UserSubscriptionsController(
            IUserSubscriptionService userSubscriptionService,
            ILogger<UserSubscriptionsController> logger)
        {
            _userSubscriptionService = userSubscriptionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var subscriptions = await _userSubscriptionService.GetUserSubscriptionsAsync(userId);
            return View(subscriptions);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var subscription = await _userSubscriptionService.GetUserSubscriptionDetailsAsync(id, userId);

            if (subscription == null)
            {
                return NotFound();
            }

            return View(subscription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPending(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            try
            {
                await _userSubscriptionService.CancelPendingSubscriptionAsync(id, userId);
                TempData["SuccessMessage"] = "Đã hủy gói đăng ký đang chờ thanh toán. Bạn có thể đăng ký gói mới.";
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "User {UserId} failed to cancel pending subscription {SubscriptionId}", userId, id);
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling pending subscription {SubscriptionId} for user {UserId}", id, userId);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy gói đăng ký. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
