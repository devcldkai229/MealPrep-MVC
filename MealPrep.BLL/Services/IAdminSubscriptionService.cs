using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IAdminSubscriptionService
    {
        Task<List<Subscription>> GetSubscriptionsAsync(string? search, SubscriptionStatus? status, int page, int pageSize);
        Task<int> GetSubscriptionsCountAsync(string? search, SubscriptionStatus? status);
        Task<Subscription?> GetSubscriptionDetailsAsync(int id);
        Task UpdateSubscriptionStatusAsync(int id, SubscriptionStatus status);
        Task CancelSubscriptionAsync(int id);
    }
}
