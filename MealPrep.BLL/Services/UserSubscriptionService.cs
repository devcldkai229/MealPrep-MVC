using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class UserSubscriptionService : IUserSubscriptionService
    {
        private readonly AppDbContext _context;

        public UserSubscriptionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Subscription>> GetUserSubscriptionsAsync(Guid userId)
        {
            return await _context.Set<Subscription>()
                .Include(s => s.Plan)
                .Include(s => s.DeliveryOrders.OrderBy(d => d.DeliveryDate))
                .Where(s => s.AppUserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Subscription?> GetUserSubscriptionDetailsAsync(int id, Guid userId)
        {
            return await _context.Set<Subscription>()
                .Include(s => s.Plan)
                .Include(s => s.DeliveryOrders.OrderBy(d => d.DeliveryDate))
                    .ThenInclude(d => d.DeliverySlot)
                .Include(s => s.DeliveryOrders)
                    .ThenInclude(d => d.Items)
                        .ThenInclude(i => i.Meal)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id && s.AppUserId == userId);
        }
    }
}
