using MealPrep.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IUserSubscriptionService
    {
        Task<List<Subscription>> GetUserSubscriptionsAsync(Guid userId);
        Task<Subscription?> GetUserSubscriptionDetailsAsync(int id, Guid userId);

        /// <summary>
        /// Hủy gói đăng ký đang ở trạng thái Chờ thanh toán cho user.
        /// </summary>
        Task CancelPendingSubscriptionAsync(int id, Guid userId);
    }
}
