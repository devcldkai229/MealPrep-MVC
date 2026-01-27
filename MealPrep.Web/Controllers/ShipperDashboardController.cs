using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Shipper,Admin")]
    public class ShipperDashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ShipperDashboardController> _logger;

        public ShipperDashboardController(
            AppDbContext context,
            ILogger<ShipperDashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            // Get statistics for today
            var totalOrders = await _context.Set<DeliveryOrder>()
                .Where(o => o.DeliveryDate == today)
                .CountAsync();

            var pendingOrders = await _context.Set<DeliveryOrder>()
                .Where(o => o.DeliveryDate == today && 
                           (o.Status == OrderStatus.Planned || 
                            o.Status == OrderStatus.Preparing || 
                            o.Status == OrderStatus.Delivering))
                .CountAsync();

            var deliveredOrders = await _context.Set<DeliveryOrder>()
                .Where(o => o.DeliveryDate == today && o.Status == OrderStatus.Delivered)
                .CountAsync();

            var totalItems = await _context.Set<DeliveryOrderItem>()
                .Include(i => i.DeliveryOrder)
                .Where(i => i.DeliveryOrder!.DeliveryDate == today)
                .CountAsync();

            var deliveredItems = await _context.Set<DeliveryOrderItem>()
                .Include(i => i.DeliveryOrder)
                .Where(i => i.DeliveryOrder!.DeliveryDate == today && i.DeliveredAt.HasValue)
                .CountAsync();

            ViewBag.TotalOrders = totalOrders;
            ViewBag.PendingOrders = pendingOrders;
            ViewBag.DeliveredOrders = deliveredOrders;
            ViewBag.TotalItems = totalItems;
            ViewBag.DeliveredItems = deliveredItems;
            ViewBag.Today = today;

            return View();
        }
    }
}
