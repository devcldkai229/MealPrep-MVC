using System.ComponentModel.DataAnnotations;

namespace MealPrep.Web.ViewModels;

public class CheckoutVm
{
    [Required]
    public int PlanId { get; set; }

    [Required]
    public int TierId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}
