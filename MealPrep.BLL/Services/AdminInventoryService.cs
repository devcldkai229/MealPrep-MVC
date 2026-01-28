using MealPrep.DAL.Data;
using BusinessObjects.Entities;
using MealPrep.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class AdminInventoryService : IAdminInventoryService
    {
        private readonly IRepository<KitchenInventory> _inventoryRepo;
        private readonly IRepository<DeliveryOrderItem> _orderItemRepo;
        private readonly AppDbContext _dbContext;
        private readonly ILogger<AdminInventoryService> _logger;

        public AdminInventoryService(
            IRepository<KitchenInventory> inventoryRepo,
            IRepository<DeliveryOrderItem> orderItemRepo,
            AppDbContext dbContext,
            ILogger<AdminInventoryService> logger)
        {
            _inventoryRepo = inventoryRepo;
            _orderItemRepo = orderItemRepo;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<List<KitchenInventory>> GetInventoryByDateAsync(DateOnly date)
        {
            return await _inventoryRepo.Query()
                .Include(i => i.Meal)
                .Where(i => i.Date == date)
                .OrderBy(i => i.Meal!.Name)
                .ToListAsync();
        }

        public async Task<List<KitchenInventory>> GetInventoryByDateRangeAsync(DateOnly startDate, DateOnly endDate)
        {
            return await _inventoryRepo.Query()
                .Include(i => i.Meal)
                .Where(i => i.Date >= startDate && i.Date <= endDate)
                .OrderBy(i => i.Date)
                .ThenBy(i => i.Meal!.Name)
                .ToListAsync();
        }

        public async Task<KitchenInventory?> GetInventoryByIdAsync(int id)
        {
            return await _inventoryRepo.Query()
                .Include(i => i.Meal)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<int> SaveInventoryAsync(KitchenInventory inventory)
        {
            if (inventory.Id == 0)
            {
                // Create new
                inventory.CreatedAt = DateTime.UtcNow;
                await _inventoryRepo.AddAsync(inventory);
            }
            else
            {
                // Update existing
                var existing = await _inventoryRepo.GetByIdAsync(inventory.Id);
                if (existing == null)
                {
                    throw new InvalidOperationException($"Inventory record with ID {inventory.Id} not found.");
                }

                existing.Date = inventory.Date;
                existing.MealId = inventory.MealId;
                existing.QuantityLimit = inventory.QuantityLimit;
                existing.Notes = inventory.Notes;
                existing.UpdatedAt = DateTime.UtcNow;

                _inventoryRepo.Update(existing);
            }

            await _inventoryRepo.SaveChangesAsync();
            return inventory.Id;
        }

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            var inventory = await _inventoryRepo.GetByIdAsync(id);
            if (inventory == null)
            {
                return false;
            }

            _inventoryRepo.Remove(inventory);
            await _inventoryRepo.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUsedQuantityAsync(int mealId, DateOnly date)
        {
            return await _dbContext.Set<DeliveryOrderItem>()
                .Include(i => i.DeliveryOrder)
                .Where(i => i.DeliveryOrder!.DeliveryDate == date && i.MealId == mealId)
                .SumAsync(i => i.Quantity);
        }

        public async Task<int?> GetAvailableQuantityAsync(int mealId, DateOnly date)
        {
            var inventory = await _inventoryRepo.Query()
                .FirstOrDefaultAsync(i => i.Date == date && i.MealId == mealId);

            if (inventory == null)
            {
                return null; // No limit set
            }

            var used = await GetUsedQuantityAsync(mealId, date);
            return Math.Max(0, inventory.QuantityLimit - used);
        }

        public async Task<bool> ExistsForDateAndMealAsync(DateOnly date, int mealId, int? excludeId = null)
        {
            var query = _inventoryRepo.Query()
                .Where(i => i.Date == date && i.MealId == mealId);

            if (excludeId.HasValue)
            {
                query = query.Where(i => i.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<(int Created, int Updated, int TotalMeals, int TotalDays)> BulkCreateInventoriesAsync(
            DateOnly startDate,
            DateOnly endDate,
            int[] mealIds,
            int quantityLimit)
        {
            if (mealIds == null || mealIds.Length == 0)
            {
                throw new ArgumentException("Danh sách món ăn trống.", nameof(mealIds));
            }

            if (startDate > endDate)
            {
                throw new ArgumentException("Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc.");
            }

            var totalCreated = 0;
            var totalUpdated = 0;
            var totalDays = (endDate.DayNumber - startDate.DayNumber) + 1;
            var totalMeals = mealIds.Length;

            foreach (var mealId in mealIds)
            {
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var existing = await _inventoryRepo.Query()
                        .FirstOrDefaultAsync(i => i.Date == date && i.MealId == mealId);

                    if (existing != null)
                    {
                        existing.QuantityLimit = quantityLimit;
                        existing.UpdatedAt = DateTime.UtcNow;
                        _inventoryRepo.Update(existing);
                        totalUpdated++;
                    }
                    else
                    {
                        var inventory = new KitchenInventory
                        {
                            Date = date,
                            MealId = mealId,
                            QuantityLimit = quantityLimit,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _inventoryRepo.AddAsync(inventory);
                        totalCreated++;
                    }
                }
            }

            await _inventoryRepo.SaveChangesAsync();

            return (totalCreated, totalUpdated, totalMeals, totalDays);
        }
    }
}
