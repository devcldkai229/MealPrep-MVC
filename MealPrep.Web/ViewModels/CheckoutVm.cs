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
    
    public DateOnly? EndDate { get; set; } // Optional, will be calculated from StartDate + Plan.DurationDays
}
