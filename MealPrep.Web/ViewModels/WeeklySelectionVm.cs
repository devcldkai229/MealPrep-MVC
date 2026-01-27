using MealPrep.DAL.Entities;

namespace MealPrep.Web.ViewModels;

public class WeeklySelectionVm
{
    public int SubscriptionId { get; set; }
    public int MealsPerDay { get; set; }
    public List<DailySlot> DailySlots { get; set; } = new();
    public List<string> UserAllergies { get; set; } = new();
    public string? DeliveryAddress { get; set; } // Địa chỉ giao hàng của user
    public bool HasDeliveryAddress { get; set; } // True nếu user đã có địa chỉ
}

public class DailySlot
{
    public DateOnly Date { get; set; }
    public string DayName { get; set; } = string.Empty;
    public int DayOfWeek { get; set; } // 1=Monday, 7=Sunday
    public List<MealOption> AvailableMeals { get; set; } = new();
    public int SlotsCount { get; set; } // Based on Subscription.MealsPerDay
    
    // Order Locking Information
    public bool IsLocked { get; set; } // True if date is past or has confirmed order
    public bool HasOrder { get; set; } // True if DeliveryOrder exists with items
    public bool IsPastCutoff { get; set; } // True if date is tomorrow and current time > 20:00 (cut-off time)
    public List<LockedMealInfo> LockedMeals { get; set; } = new(); // Meals already ordered for this date
}

public class LockedMealInfo
{
    public int MealId { get; set; }
    public string MealName { get; set; } = string.Empty;
    public int SlotIndex { get; set; } // 0 = Morning, 1 = Afternoon, etc.
}

public class MealOption
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
    public bool IsSoldOut { get; set; } // True if meal reached inventory limit for the date
    public int? AvailableQuantity { get; set; } // Remaining quantity (null if no limit set)
}

public class MealSelectionRequest
{
    public DateOnly Date { get; set; }
    public List<int> SelectedMealIds { get; set; } = new();
    /// <summary>
    /// Dictionary: SlotIndex -> MealId
    /// SlotIndex 0 = Morning, 1 = Evening, etc.
    /// </summary>
    public Dictionary<int, int> SelectedMealsBySlot { get; set; } = new();
    public string? DeliveryAddress { get; set; } // Địa chỉ giao hàng (optional, sẽ lấy từ user nếu không có)
}
