using MealPrep.BLL.DTOs;
using MealPrep.DAL.Enums;

namespace MealPrep.BLL.Services
{
    /// <summary>
    /// Service xử lý Daily Delivery Processing (Flow 5)
    /// </summary>
    public interface IDeliveryProcessingService
    {
        /// <summary>
        /// [Cron Job] Generate DeliveryOrders cho ngày mai từ Active Subscriptions
        /// Chạy tự động lúc 10h tối hàng ngày
        /// </summary>
        /// <param name="targetDate">Ngày giao hàng (mặc định = ngày mai)</param>
        /// <returns>Kết quả xử lý</returns>
        Task<GenerateDeliveryOrdersResult> GenerateDeliveryOrdersForDateAsync(DateOnly? targetDate = null);

        /// <summary>
        /// Lấy Kitchen List - Danh sách tổng hợp món cần nấu theo ngày
        /// </summary>
        Task<KitchenExportDto> GetKitchenListAsync(DateOnly date);

        /// <summary>
        /// Lấy danh sách DeliveryOrders theo ngày và trạng thái
        /// </summary>
        Task<List<DeliveryOrderDetailDto>> GetDeliveryOrdersByDateAsync(
            DateOnly date, 
            OrderStatus? status = null);

        /// <summary>
        /// Cập nhật trạng thái DeliveryOrder
        /// </summary>
        Task<bool> UpdateDeliveryOrderStatusAsync(int deliveryOrderId, OrderStatus newStatus);

        /// <summary>
        /// Bulk update trạng thái nhiều DeliveryOrders
        /// </summary>
        Task<int> BulkUpdateDeliveryOrderStatusAsync(List<int> deliveryOrderIds, OrderStatus newStatus);
    }
}
