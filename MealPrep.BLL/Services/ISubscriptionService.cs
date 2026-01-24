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
    }
}
