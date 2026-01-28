using MealPrep.DAL.Data;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class ShipperService : IShipperService
    {
        private readonly AppDbContext _context;
        private readonly IS3Service _s3Service;
        private readonly ILogger<ShipperService> _logger;

        public ShipperService(
            AppDbContext context,
            IS3Service s3Service,
            ILogger<ShipperService> logger)
        {
            _context = context;
            _s3Service = s3Service;
            _logger = logger;
        }

        public async Task<List<DeliveryOrder>> GetOrdersForDateAsync(Guid userId, DateOnly date, bool isAdmin)
        {
            IQueryable<DeliveryOrder> query = _context.DeliveryOrders
                .Include(o => o.Subscription)!.ThenInclude(s => s!.AppUser)
                .Include(o => o.Items)!.ThenInclude(i => i.Meal)
                .Include(o => o.Items)!.ThenInclude(i => i.DeliverySlot);

            if (!isAdmin)
            {
                // For shipper: Show all orders assigned to them, regardless of delivery date
                // This allows shipper to see future orders that have been assigned to them
                query = query.Where(o => o.ShipperId == userId);
                
                // Debug logging - check all assigned orders
                var allAssignedOrders = await _context.DeliveryOrders
                    .Where(o => o.ShipperId == userId)
                    .Select(o => new { o.Id, o.ShipperId, o.DeliveryDate })
                    .ToListAsync();
                
                _logger.LogInformation(
                    "🔍 Shipper {UserId} requesting orders. Total assigned orders: {Total}, Orders for date {Date}: {ForDate}",
                    userId, allAssignedOrders.Count,
                    date, allAssignedOrders.Count(o => o.DeliveryDate == date));
            }
            else
            {
                // For admin: Filter by date as before
                query = query.Where(o => o.DeliveryDate == date);
            }

            var result = await query
                .OrderBy(o => o.DeliveryDate)
                .ThenBy(o => o.Status)
                .ThenBy(o => o.Subscription!.CustomerName)
                .ToListAsync();
            
            _logger.LogInformation(
                "📦 Returning {Count} orders for userId {UserId}, date {Date}, isAdmin: {IsAdmin}",
                result.Count, userId, date, isAdmin);

            return result;
        }

        public async Task<DeliveryOrder?> GetOrderDetailsAsync(Guid userId, int orderId, bool isAdmin)
        {
            var query = _context.DeliveryOrders
                .Include(o => o.Subscription)!.ThenInclude(s => s!.AppUser)
                .Include(o => o.Items)!.ThenInclude(i => i.Meal)
                .Include(o => o.Items)!.ThenInclude(i => i.DeliverySlot)
                .Where(o => o.Id == orderId);

            if (!isAdmin)
            {
                query = query.Where(o => o.ShipperId == userId);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ShipperDashboardStatsDto> GetDashboardStatsAsync(Guid userId, DateOnly date, bool isAdmin)
        {
            var ordersQuery = _context.DeliveryOrders.AsQueryable();

            if (!isAdmin)
            {
                // For shipper: Show stats for all assigned orders, not just today
                ordersQuery = ordersQuery.Where(o => o.ShipperId == userId);
            }
            else
            {
                // For admin: Filter by date as before
                ordersQuery = ordersQuery.Where(o => o.DeliveryDate == date);
            }

            var totalOrders = await ordersQuery.CountAsync();

            var pendingOrders = await ordersQuery
                .Where(o =>
                    o.Status == OrderStatus.Planned ||
                    o.Status == OrderStatus.Preparing ||
                    o.Status == OrderStatus.Delivering)
                .CountAsync();

            var deliveredOrders = await ordersQuery
                .Where(o => o.Status == OrderStatus.Delivered)
                .CountAsync();

            var itemsQuery = _context.DeliveryOrderItems
                .Include(i => i.DeliveryOrder)
                .AsQueryable();

            if (!isAdmin)
            {
                // For shipper: Show stats for all items in assigned orders
                itemsQuery = itemsQuery.Where(i => i.DeliveryOrder!.ShipperId == userId);
            }
            else
            {
                // For admin: Filter by date as before
                itemsQuery = itemsQuery.Where(i => i.DeliveryOrder!.DeliveryDate == date);
            }

            var totalItems = await itemsQuery.CountAsync();

            var deliveredItems = await itemsQuery
                .Where(i => i.DeliveredAt.HasValue)
                .CountAsync();

            return new ShipperDashboardStatsDto(
                totalOrders,
                pendingOrders,
                deliveredOrders,
                totalItems,
                deliveredItems);
        }

        public async Task<UploadProofResult> UploadDeliveryProofAsync(
            Guid userId,
            int deliveryOrderItemId,
            Stream fileStream,
            string fileName,
            string contentType,
            long fileLength,
            bool isAdmin)
        {
            try
            {
                if (fileLength <= 0)
                {
                    return new UploadProofResult(false, "Vui lòng chọn ảnh để upload.", null);
                }

                if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    return new UploadProofResult(false, "File phải là hình ảnh.", null);
                }

                if (fileLength > 10 * 1024 * 1024)
                {
                    return new UploadProofResult(false, "Kích thước file không được vượt quá 10MB.", null);
                }

                var orderItem = await _context.DeliveryOrderItems
                    .Include(i => i.DeliveryOrder)
                    .FirstOrDefaultAsync(i => i.Id == deliveryOrderItemId);

                if (orderItem == null)
                {
                    return new UploadProofResult(false, "Không tìm thấy đơn hàng.", null);
                }

                // Nếu là shipper thì kiểm tra quyền trên đơn hàng
                if (!isAdmin && orderItem.DeliveryOrder?.ShipperId != userId)
                {
                    return new UploadProofResult(false, "Bạn không có quyền cập nhật đơn hàng này.", null);
                }

                // Upload ảnh lên S3
                var s3Key = await _s3Service.UploadFileAsync(
                    fileStream,
                    fileName,
                    "delivery-proofs",
                    contentType);

                // Cập nhật DeliveryOrderItem
                orderItem.ImageS3Key = s3Key;
                orderItem.DeliveredAt = DateTime.UtcNow;
                _context.DeliveryOrderItems.Update(orderItem);

                // Cập nhật trạng thái DeliveryOrder nếu cần
                var deliveryOrder = orderItem.DeliveryOrder;
                if (deliveryOrder != null)
                {
                    var allItems = await _context.DeliveryOrderItems
                        .Where(i => i.DeliveryOrderId == deliveryOrder.Id)
                        .ToListAsync();

                    var allDelivered = allItems.All(i => i.DeliveredAt.HasValue);
                    if (allDelivered && deliveryOrder.Status != OrderStatus.Delivered)
                    {
                        deliveryOrder.Status = OrderStatus.Delivered;
                        deliveryOrder.UpdatedAt = DateTime.UtcNow;
                        _context.DeliveryOrders.Update(deliveryOrder);
                    }
                    else if (!allDelivered && deliveryOrder.Status == OrderStatus.Planned)
                    {
                        deliveryOrder.Status = OrderStatus.Delivering;
                        deliveryOrder.UpdatedAt = DateTime.UtcNow;
                        _context.DeliveryOrders.Update(deliveryOrder);
                    }
                }

                await _context.SaveChangesAsync();

                // Generate presigned URL để hiển thị ảnh ngay sau khi upload
                var presignedUrl = _s3Service.GetPresignedUrl(s3Key, expirationHours: 24);

                _logger.LogInformation(
                    "Shipper {UserId} uploaded delivery proof for DeliveryOrderItem {ItemId}, S3Key: {S3Key}",
                    userId, deliveryOrderItemId, s3Key);

                return new UploadProofResult(
                    true, 
                    "Đã upload ảnh bằng chứng thành công!", 
                    deliveryOrder?.Id,
                    presignedUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error uploading delivery proof image for DeliveryOrderItem {ItemId} by user {UserId}",
                    deliveryOrderItemId, userId);
                return new UploadProofResult(false, $"Lỗi khi upload ảnh: {ex.Message}", null);
            }
        }

        public async Task<CompleteOrderResult> CompleteOrderAsync(Guid userId, int orderId, bool isAdmin)
        {
            try
            {
                var order = await _context.DeliveryOrders
                    .Include(o => o.Items)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return new CompleteOrderResult(false, "Không tìm thấy đơn hàng.");
                }

                if (!isAdmin && order.ShipperId != userId)
                {
                    return new CompleteOrderResult(false, "Bạn không có quyền hoàn thành đơn hàng này.");
                }

                var allItemsHaveProof = order.Items.All(i =>
                    !string.IsNullOrWhiteSpace(i.ImageS3Key) && i.DeliveredAt.HasValue);

                if (!allItemsHaveProof)
                {
                    return new CompleteOrderResult(false,
                        "Vui lòng upload ảnh bằng chứng cho tất cả các món ăn trước khi hoàn thành đơn hàng.");
                }

                order.Status = OrderStatus.Delivered;
                order.UpdatedAt = DateTime.UtcNow;
                _context.DeliveryOrders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Order {OrderId} completed by user {UserId}", orderId, userId);

                return new CompleteOrderResult(true, "Đã hoàn thành đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing delivery order {OrderId} by user {UserId}", orderId, userId);
                return new CompleteOrderResult(false, $"Lỗi khi hoàn thành đơn hàng: {ex.Message}");
            }
        }
    }
}

