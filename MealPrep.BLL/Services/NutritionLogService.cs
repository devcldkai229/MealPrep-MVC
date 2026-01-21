using MealPrep.DAL.Entities;
using MealPrep.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class NutritionLogService : INutritionLogService
    {
        private readonly IRepository<NutritionLog> _logRepo;

        public NutritionLogService(IRepository<NutritionLog> logRepo)
        {
            _logRepo = logRepo;
        }

        public async Task<List<NutritionLog>> ListAsync(Guid userId, DateOnly? date)
        {
            var q = _logRepo.Query()
                .Include(x => x.Meal)
                .Where(x => x.AppUserId == userId);

            if (date != null) q = q.Where(x => x.Date == date.Value);

            return await q.OrderByDescending(x => x.Date).ToListAsync();
        }

        public async Task CreateAsync(Guid userId, string email, NutritionLog log)
        {
            log.AppUserId = userId;
            log.CustomerEmail = email;

            await _logRepo.AddAsync(log);
            await _logRepo.SaveChangesAsync();
        }

        public async Task<List<DailySummary>> SummaryAsync(Guid userId, DateOnly from, DateOnly to)
        {
            var logs = await _logRepo.Query()
                .Include(x => x.Meal)
                .Where(x => x.AppUserId == userId && x.Date >= from && x.Date <= to)
                .ToListAsync();

            return logs
                .GroupBy(x => x.Date)
                .Select(g => new DailySummary(
                    g.Key,
                    g.Sum(x => (x.Meal!.Calories * x.Quantity)),
                    g.Sum(x => (x.Meal!.Protein * x.Quantity)),
                    g.Sum(x => (x.Meal!.Carbs * x.Quantity)),
                    g.Sum(x => (x.Meal!.Fat * x.Quantity))
                ))
                .OrderByDescending(x => x.Date)
                .ToList();
        }
    }
}
