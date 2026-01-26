using MealPrep.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IMealService
    {
        Task<List<Meal>> SearchAsync(string? q, string? sort);
        Task<(List<Meal> Meals, int TotalCount)> SearchWithPaginationAsync(string? q, string? sort, int page, int pageSize);
        Task<Meal?> GetAsync(int id);
        Task<List<Meal>> GetAllActiveMealsAsync();
        Task<List<Meal>> GetAllMealsAsync(); // For admin: includes inactive meals
        Task<(List<Meal> Meals, int TotalCount)> GetMealsForAdminAsync(string? search, bool? isActive, int page, int pageSize); // Admin filtering with pagination
        Task CreateAsync(Meal meal);
        Task UpdateAsync(Meal meal);
        Task DeleteAsync(int id);
    }
}
