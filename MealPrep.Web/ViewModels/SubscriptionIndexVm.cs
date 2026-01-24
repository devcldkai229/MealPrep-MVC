using MealPrep.DAL.Entities;

namespace MealPrep.Web.ViewModels;

public class SubscriptionIndexVm
{
    public List<Plan> Plans { get; set; } = new();
}

public class PlanWithTiersVm
{
    public Plan Plan { get; set; } = null!;
    public List<PlanMealTier> Tiers { get; set; } = new();
    public decimal GetTotalPrice(int tierId)
    {
        var tier = Tiers.FirstOrDefault(t => t.Id == tierId);
        return tier != null ? Plan.BasePrice + tier.ExtraPrice : Plan.BasePrice;
    }
}
