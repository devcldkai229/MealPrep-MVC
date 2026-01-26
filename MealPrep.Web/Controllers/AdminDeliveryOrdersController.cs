using MealPrep.BLL.Services;
using MealPrep.DAL.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDeliveryOrdersController : Controller
    {
        private readonly IAdminDeliveryOrderService _adminDeliveryOrderService;
        private readonly ILogger<AdminDeliveryOrdersController> _logger;

        public AdminDeliveryOrdersController(
            IAdminDeliveryOrderService adminDeliveryOrderService,
            ILogger<AdminDeliveryOrdersController> logger)
        {
            _adminDeliveryOrderService = adminDeliveryOrderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            OrderStatus? status,
            DateOnly? fromDate,
            DateOnly? toDate,
            int page = 1,
            int pageSize = 20)
        {
            var totalCount = await _adminDeliveryOrderService.GetDeliveryOrdersCountAsync(search, status, fromDate, toDate);
            var orders = await _adminDeliveryOrderService.GetDeliveryOrdersAsync(search, status, fromDate, toDate, page, pageSize);

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _adminDeliveryOrderService.GetDeliveryOrderDetailsAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            try
            {
                await _adminDeliveryOrderService.UpdateDeliveryOrderStatusAsync(id, status);
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{id}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating delivery order status {OrderId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi cập nhật trạng thái: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
