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
    public class MealService : IMealService
    {
        private readonly IRepository<Meal> _repo;

        public MealService(IRepository<Meal> repo) => _repo = repo;

        public async Task<List<Meal>> SearchAsync(string? q, string? sort)
        {
            var query = _repo.Query().Where(m => m.IsActive);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(m => m.Name.Contains(q) || (m.Description ?? "").Contains(q));

            query = sort switch
            {
                "cal_asc" => query.OrderBy(x => x.Calories),
                "cal_desc" => query.OrderByDescending(x => x.Calories),
                "price_asc" => query.OrderBy(x => x.Price),
                "price_desc" => query.OrderByDescending(x => x.Price),
                _ => query.OrderBy(x => x.Name)
            };

            return await query.ToListAsync();
        }

        public async Task<Meal?> GetAsync(int id) => await _repo.GetByIdAsync(id) as Meal;

        public async Task CreateAsync(Meal meal)
        {
            await _repo.AddAsync(meal);
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateAsync(Meal meal)
        {
            _repo.Update(meal);
            await _repo.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var meal = await _repo.GetByIdAsync(id) as Meal;
            if (meal == null) return;
            _repo.Remove(meal);
            await _repo.SaveChangesAsync();
        }
    }
}
