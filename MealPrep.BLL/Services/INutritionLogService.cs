using MealPrep.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public record DailySummary(DateOnly Date, int Calories, decimal Protein, decimal Carbs, decimal Fat);

    public interface INutritionLogService
    {
        Task<List<NutritionLog>> ListAsync(Guid userId, DateOnly? date);
        Task CreateAsync(Guid userId, string email, NutritionLog log);
        Task<List<DailySummary>> SummaryAsync(Guid userId, DateOnly from, DateOnly to);
    }
}
