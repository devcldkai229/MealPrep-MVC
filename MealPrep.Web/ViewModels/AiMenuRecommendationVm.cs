namespace MealPrep.Web.ViewModels;

public class AiMenuRecommendationVm
{
    public int SubscriptionId { get; set; }
    public int MealsPerDay { get; set; }
    public List<string> UserAllergies { get; set; } = new();
    
    // AI Recommendations (7 days)
    public List<AiDayRecommendation> AiRecommendations { get; set; } = new();
    
    // All available meals for user to select from (right side)
    public List<MealOption> AvailableMeals { get; set; } = new();
    
    // Week info
    public DateOnly WeekStart { get; set; }
    public DateOnly WeekEnd { get; set; }
}

public class AiDayRecommendation
{
    public int Day { get; set; } // 1-7 (Monday-Sunday)
    public string DayName { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public List<int> RecommendedMealIds { get; set; } = new();
    public string Reason { get; set; } = string.Empty;
    
    // Meal details for display
    public List<MealOption> RecommendedMeals { get; set; } = new();
}
