using MealPrep.BLL.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IMenuService
    {
        /// <summary>
        /// Lấy thông tin meal selection cho user với active subscription
        /// </summary>
        Task<WeeklySelectionDto?> GetWeeklySelectionAsync(Guid userId, DateOnly? startDate = null);
        
        /// <summary>
        /// Get meals for selection with search and pagination
        /// </summary>
        Task<(List<MealOptionDto> Meals, int TotalCount)> GetMealsForSelectionAsync(string? search, int page, int pageSize, Guid userId, DateOnly? date = null);

        /// <summary>
        /// Lưu meal selections của user
        /// </summary>
        Task SaveMealSelectionsAsync(Guid userId, int subscriptionId, List<MealSelectionRequestDto> selections);

        /// <summary>
        /// Check if meal contains allergens for user
        /// </summary>
        bool CheckAllergen(string ingredients, List<string> userAllergies);

        /// <summary>
        /// Get allergen warning message for meal
        /// </summary>
        string GetAllergenWarning(string ingredients, List<string> userAllergies);
    }
}
