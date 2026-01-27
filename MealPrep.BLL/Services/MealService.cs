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
        private readonly IMealEmbeddingService? _embeddingService;

        public MealService(IRepository<Meal> repo, IMealEmbeddingService? embeddingService = null)
        {
            _repo = repo;
            _embeddingService = embeddingService; // Optional: Only generate if service is registered
        }

        public async Task<List<Meal>> SearchAsync(string? q, string? sort)
        {
            var query = _repo.Query().Where(m => m.IsActive);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(m => m.Name.Contains(q) || (m.Description ?? "").Contains(q));

            query = sort switch
            {
                "cal_asc" => query.OrderBy(x => x.Calories),
                "cal_desc" => query.OrderByDescending(x => x.Calories),
                "price_asc" => query.OrderBy(x => x.BasePrice),
                "price_desc" => query.OrderByDescending(x => x.BasePrice),
                _ => query.OrderBy(x => x.Name)
            };

            return await query.ToListAsync();
        }

        public async Task<(List<Meal> Meals, int TotalCount)> SearchWithPaginationAsync(string? q, string? sort, int page, int pageSize)
        {
            var query = _repo.Query().Where(m => m.IsActive);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(m => m.Name.Contains(q) || (m.Description ?? "").Contains(q));

            var totalCount = await query.CountAsync();

            query = sort switch
            {
                "cal_asc" => query.OrderBy(x => x.Calories),
                "cal_desc" => query.OrderByDescending(x => x.Calories),
                "price_asc" => query.OrderBy(x => x.BasePrice),
                "price_desc" => query.OrderByDescending(x => x.BasePrice),
                _ => query.OrderBy(x => x.Name)
            };

            var meals = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (meals, totalCount);
        }

        public async Task<Meal?> GetAsync(int id) => await _repo.GetByIdAsync(id) as Meal;

        public async Task<List<Meal>> GetAllActiveMealsAsync()
        {
            return await _repo.Query()
                .Where(m => m.IsActive)
                .ToListAsync();
        }

        public async Task<List<Meal>> GetAllMealsAsync()
        {
            return await _repo.Query()
                .ToListAsync();
        }

        public async Task<(List<Meal> Meals, int TotalCount)> GetMealsForAdminAsync(string? search, bool? isActive, int page, int pageSize)
        {
            var query = _repo.Query().AsQueryable();

            // Filter by search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                var pattern = $"%{search}%";

                query = query.Where(m =>
                    EF.Functions.Like(m.Name, pattern) ||
                    (m.Description != null && EF.Functions.Like(m.Description, pattern)));
            }

            // Filter by active status if specified
            if (isActive.HasValue)
            {
                query = query.Where(m => m.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var meals = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (meals, totalCount);
        }

        public async Task CreateAsync(Meal meal)
        {
            // Set CreatedAt if not set
            if (meal.CreatedAt == default)
            {
                meal.CreatedAt = DateTime.UtcNow;
            }

            // Generate embedding if service is available (Phase 1 requirement)
            if (_embeddingService != null && meal.IsActive)
            {
                meal.EmbeddingJson = await _embeddingService.GenerateEmbeddingAsync(meal);
            }
            
            await _repo.AddAsync(meal);
            await _repo.SaveChangesAsync();
        }

        public async Task UpdateAsync(Meal meal)
        {
            // Load existing meal from database
            var existingMeal = await _repo.GetByIdAsync(meal.Id) as Meal;
            if (existingMeal == null)
            {
                throw new Exception($"Meal with ID {meal.Id} not found");
            }

            // Update properties
            existingMeal.Name = meal.Name;
            existingMeal.Description = meal.Description;
            existingMeal.Ingredients = meal.Ingredients;
            existingMeal.Images = meal.Images;
            existingMeal.Calories = meal.Calories;
            existingMeal.Protein = meal.Protein;
            existingMeal.Carbs = meal.Carbs;
            existingMeal.Fat = meal.Fat;
            existingMeal.BasePrice = meal.BasePrice;
            existingMeal.IsActive = meal.IsActive;
            existingMeal.UpdatedAt = DateTime.UtcNow;

            // Regenerate embedding if service is available and meal is active
            if (_embeddingService != null && existingMeal.IsActive)
            {
                existingMeal.EmbeddingJson = await _embeddingService.GenerateEmbeddingAsync(existingMeal);
            }
            
            _repo.Update(existingMeal);
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
