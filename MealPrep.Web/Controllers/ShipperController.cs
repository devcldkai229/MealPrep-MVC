using MealPrep.BLL.Services;
using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MealPrep.Web.Controllers
{
    /// <summary>
    /// Controller for Shipper operations (upload delivery proof images)
    /// </summary>
    [Authorize(Roles = "Admin,Shipper")] // Allow both Admin and Shipper roles
    [ApiController]
    [Route("api/[controller]")]
    public class ShipperController : ControllerBase
    {
        private readonly IS3Service _s3Service;
        private readonly AppDbContext _context;
        private readonly ILogger<ShipperController> _logger;

        public ShipperController(
            IS3Service s3Service,
            AppDbContext context,
            ILogger<ShipperController> logger)
        {
            _s3Service = s3Service;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Upload delivery proof image for a DeliveryOrderItem
        /// </summary>
        [HttpPost("upload-delivery-proof")]
        public async Task<IActionResult> UploadDeliveryProof([FromForm] int deliveryOrderItemId, [FromForm] IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Vui lòng chọn ảnh để upload." });
                }

                if (!image.ContentType.StartsWith("image/"))
                {
                    return BadRequest(new { success = false, message = "File phải là hình ảnh." });
                }

                // Validate file size (max 10MB)
                if (image.Length > 10 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "Kích thước file không được vượt quá 10MB." });
                }

                // Get DeliveryOrderItem
                var orderItem = await _context.Set<DeliveryOrderItem>()
                    .Include(i => i.DeliveryOrder)
                    .FirstOrDefaultAsync(i => i.Id == deliveryOrderItemId);

                if (orderItem == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });
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
                    if (allDelivered && deliveryOrder.Status != DAL.Enums.OrderStatus.Delivered)
                    {
                        deliveryOrder.Status = DAL.Enums.OrderStatus.Delivered;
                        deliveryOrder.UpdatedAt = DateTime.UtcNow;
                        _context.Set<DeliveryOrder>().Update(deliveryOrder);
                    }
                }

                await _context.SaveChangesAsync();

                // Get presigned URL for the uploaded image
                var imageUrl = _s3Service.GetPresignedUrl(s3Key, 8760); // 1 year expiration

                _logger.LogInformation("Shipper uploaded delivery proof for DeliveryOrderItem {ItemId}, S3Key: {S3Key}", 
                    deliveryOrderItemId, s3Key);

                return Ok(new
                {
                    success = true,
                    message = "Upload ảnh bằng chứng thành công!",
                    s3Key = s3Key,
                    imageUrl = imageUrl,
                    deliveredAt = orderItem.DeliveredAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading delivery proof image for DeliveryOrderItem {ItemId}", deliveryOrderItemId);
                return StatusCode(500, new { success = false, message = $"Lỗi khi upload ảnh: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get delivery proof image URL for a DeliveryOrderItem
        /// </summary>
        [HttpGet("delivery-proof/{deliveryOrderItemId}")]
        public async Task<IActionResult> GetDeliveryProof(int deliveryOrderItemId)
        {
            try
            {
                var orderItem = await _context.Set<DeliveryOrderItem>()
                    .FirstOrDefaultAsync(i => i.Id == deliveryOrderItemId);

                if (orderItem == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });
                }

                if (string.IsNullOrWhiteSpace(orderItem.ImageS3Key))
                {
                    return NotFound(new { success = false, message = "Chưa có ảnh bằng chứng cho đơn hàng này." });
                }

                // Get presigned URL
                var imageUrl = _s3Service.GetPresignedUrl(orderItem.ImageS3Key, 1); // 1 hour expiration

                return Ok(new
                {
                    success = true,
                    s3Key = orderItem.ImageS3Key,
                    imageUrl = imageUrl,
                    deliveredAt = orderItem.DeliveredAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting delivery proof for DeliveryOrderItem {ItemId}", deliveryOrderItemId);
                return StatusCode(500, new { success = false, message = $"Lỗi khi lấy ảnh: {ex.Message}" });
            }
        }
    }
}
