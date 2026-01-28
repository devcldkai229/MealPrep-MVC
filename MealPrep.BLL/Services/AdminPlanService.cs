using MealPrep.DAL.Data;
using BusinessObjects.Entities;
using BusinessObjects.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class AdminPlanService : IAdminPlanService
    {
        private readonly AppDbContext _context;

        public AdminPlanService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Plan>> GetAllPlansAsync()
        {
            return await _context.Set<Plan>()
                .Include(p => p.MealTiers)
                .Include(p => p.Subscriptions)
                .OrderBy(p => p.DurationDays)
                .ToListAsync();
        }

        public async Task<Plan?> GetPlanDetailsAsync(int id)
        {
            return await _context.Set<Plan>()
                .Include(p => p.MealTiers)
                .Include(p => p.Subscriptions)
                    .ThenInclude(s => s.AppUser)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task CreatePlanAsync(Plan plan)
        {
            await _context.Set<Plan>().AddAsync(plan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePlanAsync(int id, Plan plan)
        {
            var existingPlan = await _context.Set<Plan>().FindAsync(id);
            if (existingPlan == null)
            {
                throw new ArgumentException($"Plan with ID {id} not found");
            }

            existingPlan.Name = plan.Name;
            existingPlan.Description = plan.Description;
            existingPlan.DurationDays = plan.DurationDays;
            existingPlan.BasePrice = plan.BasePrice;
            existingPlan.IsActive = plan.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanDeletePlanAsync(int id)
        {
            return !await _context.Set<Plan>()
                .Where(p => p.Id == id)
                .SelectMany(p => p.Subscriptions)
                .AnyAsync(s => s.Status == SubscriptionStatus.Active);
        }

        public async Task DeletePlanAsync(int id)
        {
            var plan = await _context.Set<Plan>().FindAsync(id);
            if (plan == null)
            {
                throw new ArgumentException($"Plan with ID {id} not found");
            }

            _context.Set<Plan>().Remove(plan);
            await _context.SaveChangesAsync();
        }
    }
}
