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
    }
}
