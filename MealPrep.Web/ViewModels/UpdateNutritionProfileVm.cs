using BusinessObjects.Entities;
using BusinessObjects.Enums;
using System.ComponentModel.DataAnnotations;

namespace MealPrep.Web.ViewModels
{
    public class UpdateNutritionProfileVm
    {

        [Required(ErrorMessage = "Vui lòng nh?p chi?u cao")]
        [Range(80, 220, ErrorMessage = "Chi?u cao ph?i t? 80-220 cm")]
        [Display(Name = "Chi?u cao (cm)")]
        public int HeightCm { get; set; }

        [Required(ErrorMessage = "Vui lòng nh?p cân n?ng")]
        [Range(20, 250, ErrorMessage = "Cân n?ng ph?i t? 20-250 kg")]
        [Display(Name = "Cân n?ng (kg)")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "Vui lòng ch?n m?c tiêu")]
        [Display(Name = "M?c tiêu")]
        public FitnessGoal Goal { get; set; }

        [Required(ErrorMessage = "Vui lòng ch?n m?c d? ho?t d?ng")]
        [Display(Name = "M?c d? ho?t d?ng")]
        public ActivityLevel ActivityLevel { get; set; }

        [Required(ErrorMessage = "Vui lòng ch?n ch? d? an")]
        [Display(Name = "Ch? d? an")]
        public DietPreference DietPreference { get; set; }

        [Required(ErrorMessage = "Vui lòng nh?p s? b?a an")]
        [Range(1, 6, ErrorMessage = "S? b?a an ph?i t? 1-6")]
        [Display(Name = "S? b?a m?i ngày")]
        public int MealsPerDay { get; set; } = 3;

        [Range(1000, 5000, ErrorMessage = "Calories trong ngày ph?i t? 1000-5000 kcal")]
        [Display(Name = "Calories trong m?t ngày (kcal)")]
        public int? CaloriesInDay { get; set; }

        [StringLength(10000, ErrorMessage = "Ghi chú không du?c vu?t quá 10000 ký t?")]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
    }
}
