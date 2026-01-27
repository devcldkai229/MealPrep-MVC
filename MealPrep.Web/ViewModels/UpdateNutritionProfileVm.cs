using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace MealPrep.Web.ViewModels
{
    public class UpdateNutritionProfileVm
    {

        [Required(ErrorMessage = "Vui lòng nhập chiều cao")]
        [Range(80, 220, ErrorMessage = "Chiều cao phải từ 80-220 cm")]
        [Display(Name = "Chiều cao (cm)")]
        public int HeightCm { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập cân nặng")]
        [Range(20, 250, ErrorMessage = "Cân nặng phải từ 20-250 kg")]
        [Display(Name = "Cân nặng (kg)")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn mục tiêu")]
        [Display(Name = "Mục tiêu")]
        public FitnessGoal Goal { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn mức độ hoạt động")]
        [Display(Name = "Mức độ hoạt động")]
        public ActivityLevel ActivityLevel { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chế độ ăn")]
        [Display(Name = "Chế độ ăn")]
        public DietPreference DietPreference { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số bữa ăn")]
        [Range(1, 6, ErrorMessage = "Số bữa ăn phải từ 1-6")]
        [Display(Name = "Số bữa mỗi ngày")]
        public int MealsPerDay { get; set; } = 3;

        [Range(1000, 5000, ErrorMessage = "Calories trong ngày phải từ 1000-5000 kcal")]
        [Display(Name = "Calories trong một ngày (kcal)")]
        public int? CaloriesInDay { get; set; }

        [StringLength(10000, ErrorMessage = "Ghi chú không được vượt quá 10000 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }
    }
}
