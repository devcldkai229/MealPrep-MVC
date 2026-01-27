using MealPrep.DAL.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IShipperService
    {
        /// <summary>
        /// Lấy danh sách DeliveryOrder cho shipper hoặc admin theo ngày.
        /// </summary>
        Task<List<DeliveryOrder>> GetOrdersForDateAsync(Guid userId, DateOnly date, bool isAdmin);

        /// <summary>
        /// Lấy chi tiết một DeliveryOrder (kiểm tra quyền nếu là shipper).
        /// </summary>
        Task<DeliveryOrder?> GetOrderDetailsAsync(Guid userId, int orderId, bool isAdmin);

        /// <summary>
        /// Thống kê dashboard cho shipper / admin theo ngày.
        /// </summary>
        Task<ShipperDashboardStatsDto> GetDashboardStatsAsync(Guid userId, DateOnly date, bool isAdmin);

        /// <summary>
        /// Upload ảnh bằng chứng giao hàng cho một DeliveryOrderItem.
        /// </summary>
        Task<UploadProofResult> UploadDeliveryProofAsync(
            Guid userId,
            int deliveryOrderItemId,
            Stream fileStream,
            string fileName,
            string contentType,
            long fileLength,
            bool isAdmin);

        /// <summary>
        /// Hoàn thành đơn hàng (tất cả items đã có ảnh bằng chứng).
        /// </summary>
        Task<CompleteOrderResult> CompleteOrderAsync(Guid userId, int orderId, bool isAdmin);
    }

    public record ShipperDashboardStatsDto(
        int TotalOrders,
        int PendingOrders,
        int DeliveredOrders,
        int TotalItems,
        int DeliveredItems);

    public record UploadProofResult(
        bool Success,
        string Message,
        int? DeliveryOrderId);

    public record CompleteOrderResult(
        bool Success,
        string Message);
}

