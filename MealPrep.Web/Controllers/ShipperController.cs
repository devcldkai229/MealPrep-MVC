using MealPrep.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IShipperService _shipperService;
        private readonly ILogger<ShipperController> _logger;

        public ShipperController(
            IShipperService shipperService,
            ILogger<ShipperController> logger)
        {
            _shipperService = shipperService;
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
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var isAdmin = User.IsInRole("Admin");

                var result = await _shipperService.UploadDeliveryProofAsync(
                    userId,
                    deliveryOrderItemId,
                    image?.OpenReadStream() ?? Stream.Null,
                    image?.FileName ?? string.Empty,
                    image?.ContentType ?? string.Empty,
                    image?.Length ?? 0,
                    isAdmin);

                if (!result.Success)
                {
                    return BadRequest(new { success = false, message = result.Message });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    deliveryOrderId = result.DeliveryOrderId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading delivery proof image for DeliveryOrderItem {ItemId}", deliveryOrderItemId);
                return StatusCode(500, new { success = false, message = $"Lỗi khi upload ảnh: {ex.Message}" });
            }
        }
    }
}
