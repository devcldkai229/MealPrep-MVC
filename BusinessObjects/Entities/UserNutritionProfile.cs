using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class UserNutritionProfile
    {
        public int Id { get; set; }

        // FK 1-1
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;

        [Range(80, 220)]
        public int HeightCm { get; set; }

        [Range(20, 250)]
        public decimal WeightKg { get; set; }

        public FitnessGoal Goal { get; set; } = FitnessGoal.Maintain;

        public ActivityLevel ActivityLevel { get; set; } = ActivityLevel.ModeratelyActive;

        // constraint
        public DietPreference DietPreference { get; set; } = DietPreference.None;

        [Range(1, 6)]
        public int MealsPerDay { get; set; } = 3;

        public int? CaloriesInDay { get; set; }

        [StringLength(10000)]
        public string? Notes { get; set; } // b?nh lý nh?, s? thích, m?c tiêu c? th?...

        // allergies (1-n)
        public List<UserAllergy> Allergies { get; set; } = new();
    }
}
