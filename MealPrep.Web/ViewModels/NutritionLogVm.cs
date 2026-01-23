using MealPrep.DAL.Entities;
using System.ComponentModel.DataAnnotations;

namespace MealPrep.Web.ViewModels
{
    public class NutritionLogVm
    {

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public int MealId { get; set; }

        [Range(1, 10)]
        public int Quantity { get; set; } = 1;
    }
}
