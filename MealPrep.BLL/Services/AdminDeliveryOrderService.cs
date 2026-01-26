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
    public class AdminDeliveryOrderService : IAdminDeliveryOrderService
    {
        private readonly AppDbContext _context;

        public AdminDeliveryOrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DeliveryOrder>> GetDeliveryOrdersAsync(
            string? search,
            OrderStatus? status,
            DateOnly? fromDate,
            DateOnly? toDate,
            int page,
            int pageSize)
        {
            var query = _context.Set<DeliveryOrder>()
                .Include(d => d.Subscription)
                    .ThenInclude(s => s!.AppUser)
                .Include(d => d.DeliverySlot)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Meal)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    (d.Subscription != null && d.Subscription.CustomerEmail.Contains(search)) ||
                    (d.Subscription != null && d.Subscription.CustomerName.Contains(search)));
            }

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(d => d.DeliveryDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.DeliveryDate <= toDate.Value);
            }

            return await query
                .OrderByDescending(d => d.DeliveryDate)
                .ThenByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetDeliveryOrdersCountAsync(
            string? search,
            OrderStatus? status,
            DateOnly? fromDate,
            DateOnly? toDate)
        {
            var query = _context.Set<DeliveryOrder>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    (d.Subscription != null && d.Subscription.CustomerEmail.Contains(search)) ||
                    (d.Subscription != null && d.Subscription.CustomerName.Contains(search)));
            }

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(d => d.DeliveryDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.DeliveryDate <= toDate.Value);
            }

            return await query.CountAsync();
        }

        public async Task<DeliveryOrder?> GetDeliveryOrderDetailsAsync(int id)
        {
            return await _context.Set<DeliveryOrder>()
                .Include(d => d.Subscription)
                    .ThenInclude(s => s!.AppUser)
                .Include(d => d.Subscription)
                    .ThenInclude(s => s!.Plan)
                .Include(d => d.DeliverySlot)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Meal)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task UpdateDeliveryOrderStatusAsync(int id, OrderStatus status)
        {
            var order = await _context.Set<DeliveryOrder>().FindAsync(id);
            if (order == null)
            {
                throw new ArgumentException($"DeliveryOrder with ID {id} not found");
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
