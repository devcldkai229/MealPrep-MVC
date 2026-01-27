using MealPrep.BLL.Services;
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
    public class ShipperDeliveryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IS3Service _s3Service;
        private readonly ILogger<ShipperDeliveryController> _logger;

        public ShipperDeliveryController(
            AppDbContext context,
            IS3Service s3Service,
            ILogger<ShipperDeliveryController> logger)
        {
            _context = context;
            _s3Service = s3Service;
            _logger = logger;
        }

        /// <summary>
        /// Danh sách đơn hàng cần giao hôm nay
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(DateOnly? date = null)
        {
            var deliveryDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            var orders = await _context.Set<DeliveryOrder>()
                .Include(o => o.Subscription)
                    .ThenInclude(s => s!.AppUser)
                .Include(o => o.DeliverySlot)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Meal)
                .Where(o => o.DeliveryDate == deliveryDate)
                .OrderBy(o => o.Status)
                .ThenBy(o => o.Subscription!.CustomerName)
                .ToListAsync();

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
            var order = await _context.Set<DeliveryOrder>()
                .Include(o => o.Subscription)
                    .ThenInclude(s => s!.AppUser)
                .Include(o => o.DeliverySlot)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Meal)
                .FirstOrDefaultAsync(o => o.Id == id);

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
                if (image == null || image.Length == 0)
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn ảnh để upload.";
                    return RedirectToAction(nameof(Details), new { id = await GetDeliveryOrderIdFromItem(deliveryOrderItemId) });
                }

                if (!image.ContentType.StartsWith("image/"))
                {
                    TempData["ErrorMessage"] = "File phải là hình ảnh.";
                    return RedirectToAction(nameof(Details), new { id = await GetDeliveryOrderIdFromItem(deliveryOrderItemId) });
                }

                // Validate file size (max 10MB)
                if (image.Length > 10 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Kích thước file không được vượt quá 10MB.";
                    return RedirectToAction(nameof(Details), new { id = await GetDeliveryOrderIdFromItem(deliveryOrderItemId) });
                }

                // Get DeliveryOrderItem
                var orderItem = await _context.Set<DeliveryOrderItem>()
                    .Include(i => i.DeliveryOrder)
                    .FirstOrDefaultAsync(i => i.Id == deliveryOrderItemId);

                if (orderItem == null)
                {
                    return NotFound();
                }

                // Upload image to S3
                var s3Key = await _s3Service.UploadFileAsync(
                    image.OpenReadStream(),
                    image.FileName,
                    "delivery-proofs",
                    image.ContentType);

                // Update DeliveryOrderItem with S3 key and delivery timestamp
                orderItem.ImageS3Key = s3Key;
                orderItem.DeliveredAt = DateTime.UtcNow;
                _context.Set<DeliveryOrderItem>().Update(orderItem);

                // If all items in the order are delivered, update order status
                var deliveryOrder = orderItem.DeliveryOrder;
                if (deliveryOrder != null)
                {
                    var allItems = await _context.Set<DeliveryOrderItem>()
                        .Where(i => i.DeliveryOrderId == deliveryOrder.Id)
                        .ToListAsync();

                    var allDelivered = allItems.All(i => i.DeliveredAt.HasValue);
                    if (allDelivered && deliveryOrder.Status != OrderStatus.Delivered)
                    {
                        deliveryOrder.Status = OrderStatus.Delivered;
                        deliveryOrder.UpdatedAt = DateTime.UtcNow;
                        _context.Set<DeliveryOrder>().Update(deliveryOrder);
                    }
                    else if (!allDelivered && deliveryOrder.Status == OrderStatus.Planned)
                    {
                        // Mark as Delivering if at least one item is delivered
                        deliveryOrder.Status = OrderStatus.Delivering;
                        deliveryOrder.UpdatedAt = DateTime.UtcNow;
                        _context.Set<DeliveryOrder>().Update(deliveryOrder);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã upload ảnh bằng chứng thành công!";
                return RedirectToAction(nameof(Details), new { id = deliveryOrder!.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading delivery proof for DeliveryOrderItem {ItemId}", deliveryOrderItemId);
                TempData["ErrorMessage"] = $"Lỗi khi upload ảnh: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id = await GetDeliveryOrderIdFromItem(deliveryOrderItemId) });
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
                var order = await _context.Set<DeliveryOrder>()
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound();
                }

                // Check if all items have proof images
                var allItemsHaveProof = order.Items.All(i => !string.IsNullOrWhiteSpace(i.ImageS3Key) && i.DeliveredAt.HasValue);
                
                if (!allItemsHaveProof)
                {
                    TempData["ErrorMessage"] = "Vui lòng upload ảnh bằng chứng cho tất cả các món ăn trước khi hoàn thành đơn hàng.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                order.Status = OrderStatus.Delivered;
                order.UpdatedAt = DateTime.UtcNow;
                _context.Set<DeliveryOrder>().Update(order);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã hoàn thành đơn hàng thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing delivery order {OrderId}", id);
                TempData["ErrorMessage"] = $"Lỗi khi hoàn thành đơn hàng: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        private async Task<int> GetDeliveryOrderIdFromItem(int deliveryOrderItemId)
        {
            var item = await _context.Set<DeliveryOrderItem>()
                .FirstOrDefaultAsync(i => i.Id == deliveryOrderItemId);
            return item?.DeliveryOrderId ?? 0;
        }
    }
}
