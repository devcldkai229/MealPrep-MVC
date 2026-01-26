using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class AdminSubscriptionService : IAdminSubscriptionService
    {
        private readonly AppDbContext _context;

        public AdminSubscriptionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Subscription>> GetSubscriptionsAsync(string? search, SubscriptionStatus? status, int page, int pageSize)
        {
            var query = _context.Set<Subscription>()
                .Include(s => s.AppUser)
                .Include(s => s.Plan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.CustomerEmail.Contains(search) ||
                    s.CustomerName.Contains(search) ||
                    (s.AppUser != null && s.AppUser.Email.Contains(search)));
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetSubscriptionsCountAsync(string? search, SubscriptionStatus? status)
        {
            var query = _context.Set<Subscription>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.CustomerEmail.Contains(search) ||
                    s.CustomerName.Contains(search) ||
                    (s.AppUser != null && s.AppUser.Email.Contains(search)));
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            return await query.CountAsync();
        }

        public async Task<Subscription?> GetSubscriptionDetailsAsync(int id)
        {
            return await _context.Set<Subscription>()
                .Include(s => s.AppUser)
                .Include(s => s.Plan)
                .Include(s => s.DeliveryOrders)
                    .ThenInclude(d => d.DeliverySlot)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task UpdateSubscriptionStatusAsync(int id, SubscriptionStatus status)
        {
            var subscription = await _context.Set<Subscription>().FindAsync(id);
            if (subscription == null)
            {
                throw new ArgumentException($"Subscription with ID {id} not found");
            }

            subscription.Status = status;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task CancelSubscriptionAsync(int id)
        {
            var subscription = await _context.Set<Subscription>().FindAsync(id);
            if (subscription == null)
            {
                throw new ArgumentException($"Subscription with ID {id} not found");
            }

            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
