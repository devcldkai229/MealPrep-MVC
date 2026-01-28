using BusinessObjects.Entities;
using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IAdminDeliveryOrderService
    {
        Task<List<DeliveryOrder>> GetDeliveryOrdersAsync(
            string? search,
            OrderStatus? status,
            DateOnly? fromDate,
            DateOnly? toDate,
            int page,
            int pageSize);
        Task<int> GetDeliveryOrdersCountAsync(
            string? search,
            OrderStatus? status,
            DateOnly? fromDate,
            DateOnly? toDate);
        Task<DeliveryOrder?> GetDeliveryOrderDetailsAsync(int id);
        Task UpdateDeliveryOrderStatusAsync(int id, OrderStatus status);

        /// <summary>
        /// Lấy danh sách shipper đang hoạt động.
        /// </summary>
        Task<List<AppUser>> GetActiveShippersAsync();

        /// <summary>
        /// Gán hoặc bỏ gán shipper cho một DeliveryOrder.
        /// </summary>
        Task AssignShipperAsync(int deliveryOrderId, Guid? shipperId);
    }
}
