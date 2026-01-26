using MealPrep.DAL.Entities;
using System;
using System.Collections.Generic;

namespace MealPrep.BLL.DTOs
{
    public class WeeklySelectionDto
    {
        public int SubscriptionId { get; set; }
        public int MealsPerDay { get; set; }
        public List<DailySlotDto> DailySlots { get; set; } = new();
        public List<string> UserAllergies { get; set; } = new();
    }

    public class DailySlotDto
    {
        public DateOnly Date { get; set; }
        public string DayName { get; set; } = string.Empty;
        public int DayOfWeek { get; set; } // 1=Monday, 7=Sunday
        public List<MealOptionDto> AvailableMeals { get; set; } = new();
        public int SlotsCount { get; set; } // Based on Subscription.MealsPerDay
    }

    public class MealOptionDto
    {
        public int MealId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; } // First image URL from Images JSON
        public int Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbs { get; set; }
        public decimal Fat { get; set; }
        public bool HasAllergen { get; set; }
        public string AllergenWarning { get; set; } = string.Empty;
    }

    public class MealSelectionRequestDto
    {
        public DateOnly Date { get; set; }
        public List<int> SelectedMealIds { get; set; } = new();
    }
}
