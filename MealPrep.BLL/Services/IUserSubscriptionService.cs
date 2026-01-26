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
    }
}
