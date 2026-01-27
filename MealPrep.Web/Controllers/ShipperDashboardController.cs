using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Shipper,Admin")]
    public class ShipperDashboardController : Controller
    {
        private readonly IShipperService _shipperService;
        private readonly ILogger<ShipperDashboardController> _logger;

        public ShipperDashboardController(
            IShipperService shipperService,
            ILogger<ShipperDashboardController> logger)
        {
            _shipperService = shipperService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isShipper = User.IsInRole("Shipper") && !User.IsInRole("Admin");

            var stats = await _shipperService.GetDashboardStatsAsync(userId, today, !isShipper);

            ViewBag.TotalOrders = stats.TotalOrders;
            ViewBag.PendingOrders = stats.PendingOrders;
            ViewBag.DeliveredOrders = stats.DeliveredOrders;
            ViewBag.TotalItems = stats.TotalItems;
            ViewBag.DeliveredItems = stats.DeliveredItems;
            ViewBag.Today = today;

            return View();
        }
    }
}
