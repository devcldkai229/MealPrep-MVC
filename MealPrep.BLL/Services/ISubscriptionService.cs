using BusinessObjects.Entities;
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
        /// T?o subscription và payment cho checkout
        /// </summary>
        Task<(Subscription subscription, Payment payment)> CreateSubscriptionWithPaymentAsync(
            Guid userId, int planId, int tierId, DateOnly startDate);
        
        /// <summary>
        /// Xác nh?n thanh toán và kích ho?t subscription
        /// </summary>
        Task<Subscription?> ConfirmPaymentAsync(string paymentCode, string? momoOrderId = null, string? rawResponse = null);

        /// <summary>
        /// L?y subscription t? mã thanh toán (dùng khi payment dã du?c x? lý tru?c dó)
        /// </summary>
        Task<Subscription?> GetSubscriptionByPaymentCodeAsync(string paymentCode);

        /// <summary>
        /// T?o (ho?c l?y) payment m?i cho m?t subscription dang ch? thanh toán d? user thanh toán l?i.
        /// </summary>
        Task<Payment> CreateOrGetPendingPaymentAsync(int subscriptionId, Guid userId);
    }
}
