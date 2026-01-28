using MealPrep.BLL.Services;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    [Authorize(Roles = "Shipper,Admin")]
    public class ShipperDeliveryController : Controller
    {
        private readonly IShipperService _shipperService;
        private readonly ILogger<ShipperDeliveryController> _logger;

        public ShipperDeliveryController(
            IShipperService shipperService,
            ILogger<ShipperDeliveryController> logger)
        {
            _shipperService = shipperService;
            _logger = logger;
        }

        /// <summary>
        /// Danh sách đơn hàng cần giao hôm nay
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(DateOnly? date = null)
        {
            var deliveryDate = date ?? DateOnly.FromDateTime(DateTime.Today);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isShipper = User.IsInRole("Shipper") && !User.IsInRole("Admin");

            var orders = await _shipperService.GetOrdersForDateAsync(userId, deliveryDate, !isShipper);

            ViewBag.DeliveryDate = deliveryDate;
            ViewBag.Today = DateOnly.FromDateTime(DateTime.Today);

            return View(orders);
        }

        /// <summary>
        /// Chi tiết đơn hàng và upload ảnh bằng chứng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isShipper = User.IsInRole("Shipper") && !User.IsInRole("Admin");

            var order = await _shipperService.GetOrderDetailsAsync(userId, id, !isShipper);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// Upload ảnh bằng chứng cho một DeliveryOrderItem
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProof(int deliveryOrderItemId, IFormFile image)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isShipper = User.IsInRole("Shipper") && !User.IsInRole("Admin");

                var result = await _shipperService.UploadDeliveryProofAsync(
                    userId,
                    deliveryOrderItemId,
                    image?.OpenReadStream() ?? Stream.Null,
                    image?.FileName ?? string.Empty,
                    image?.ContentType ?? string.Empty,
                    image?.Length ?? 0,
                    !isShipper);

                // If AJAX request, return JSON
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    if (!result.Success)
                    {
                        return Json(new { success = false, message = result.Message });
                    }

                    return Json(new 
                    { 
                        success = true, 
                        message = result.Message,
                        imageUrl = result.ImagePresignedUrl,
                        deliveryOrderId = result.DeliveryOrderId
                    });
                }

                // Otherwise, redirect (backward compatibility)
                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    var fallbackId = result.DeliveryOrderId ?? 0;
                    return RedirectToAction(nameof(Details), new { id = fallbackId });
                }

                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Details), new { id = result.DeliveryOrderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading delivery proof for DeliveryOrderItem {ItemId}", deliveryOrderItemId);
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = $"Lỗi khi upload ảnh: {ex.Message}" });
                }

                TempData["ErrorMessage"] = $"Lỗi khi upload ảnh: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = 0 });
            }
        }

        /// <summary>
        /// Mark delivery order as completed (all items delivered)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isShipper = User.IsInRole("Shipper") && !User.IsInRole("Admin");

                var result = await _shipperService.CompleteOrderAsync(userId, id, !isShipper);

                if (!result.Success)
                {
                    TempData["ErrorMessage"] = result.Message;
                    return RedirectToAction(nameof(Details), new { id });
                }

                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing delivery order {OrderId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi hoàn thành đơn hàng: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }
    }
}
