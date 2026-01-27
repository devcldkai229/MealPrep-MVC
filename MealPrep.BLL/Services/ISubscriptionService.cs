using MealPrep.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface ISubscriptionService
    {
        Task<int> CreateAsync(Guid userId, Subscription sub, int deliverySlotId);
        Task<Subscription?> GetDetailsAsync(int id, Guid userId);
        Task<List<Plan>> GetAllPlansWithTiersAsync();
        Task<Plan?> GetPlanByIdAsync(int planId);
        Task<PlanMealTier?> GetTierByIdAsync(int tierId);
        Task<decimal> CalculateTotalPriceAsync(int planId, int tierId);
        
        /// <summary>
        /// Tạo subscription và payment cho checkout
        /// </summary>
        Task<(Subscription subscription, Payment payment)> CreateSubscriptionWithPaymentAsync(
            Guid userId, int planId, int tierId, DateOnly startDate);
        
        /// <summary>
        /// Xác nhận thanh toán và kích hoạt subscription
        /// </summary>
        Task<Subscription?> ConfirmPaymentAsync(string paymentCode, string? momoOrderId = null, string? rawResponse = null);

        /// <summary>
        /// Lấy subscription từ mã thanh toán (dùng khi payment đã được xử lý trước đó)
        /// </summary>
        Task<Subscription?> GetSubscriptionByPaymentCodeAsync(string paymentCode);

        /// <summary>
        /// Tạo (hoặc lấy) payment mới cho một subscription đang chờ thanh toán để user thanh toán lại.
        /// </summary>
        Task<Payment> CreateOrGetPendingPaymentAsync(int subscriptionId, Guid userId);
    }
}
