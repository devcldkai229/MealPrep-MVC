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
        Task<Meal?> GetAsync(int id);
        Task CreateAsync(Meal meal);
        Task UpdateAsync(Meal meal);
        Task DeleteAsync(int id);
    }
}
