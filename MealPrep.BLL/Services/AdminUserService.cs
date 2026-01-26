using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly AppDbContext _context;

        public AdminUserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AppUser>> GetUsersAsync(string? search, int page, int pageSize)
        {
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Email.Contains(search) ||
                    u.FullName.Contains(search) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
            }

            return await query
                .OrderByDescending(u => u.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUsersCountAsync(string? search)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Email.Contains(search) ||
                    u.FullName.Contains(search) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
            }

            return await query.CountAsync();
        }

        public async Task<AppUser?> GetUserDetailsAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.NutritionProfile)
                    .ThenInclude(np => np!.Allergies)
                .Include(u => u.Subscriptions)
                    .ThenInclude(s => s.Plan)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task DeactivateUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {id} not found");
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task ActivateUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new ArgumentException($"User with ID {id} not found");
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();
        }
    }
}
