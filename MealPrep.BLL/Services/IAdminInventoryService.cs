using MealPrep.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IAdminInventoryService
    {
        /// <summary>
        /// Get all inventory records for a specific date
        /// </summary>
        Task<List<KitchenInventory>> GetInventoryByDateAsync(DateOnly date);

        /// <summary>
        /// Get inventory for a date range
        /// </summary>
        Task<List<KitchenInventory>> GetInventoryByDateRangeAsync(DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Get inventory by ID
        /// </summary>
        Task<KitchenInventory?> GetInventoryByIdAsync(int id);

        /// <summary>
        /// Create or update inventory record
        /// </summary>
        Task<int> SaveInventoryAsync(KitchenInventory inventory);

        /// <summary>
        /// Delete inventory record
        /// </summary>
        Task<bool> DeleteInventoryAsync(int id);

        /// <summary>
        /// Get used quantity for a meal on a specific date (from DeliveryOrderItems)
        /// </summary>
        Task<int> GetUsedQuantityAsync(int mealId, DateOnly date);

        /// <summary>
        /// Get available quantity (limit - used)
        /// </summary>
        Task<int?> GetAvailableQuantityAsync(int mealId, DateOnly date);
    }
}
