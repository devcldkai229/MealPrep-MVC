using System;

namespace MealPrep.BLL.DTOs
{
    public class KitchenPrepForecastDto
    {
        public int MealId { get; set; }
        public string MealName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class RevenueGrowthDto
    {
        public DateOnly Date { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SubscriptionGrowthDto
    {
        public DateOnly Date { get; set; }
        public int NewSubscriptions { get; set; }
    }

    public class TopDislikedMealDto
    {
        public int MealId { get; set; }
        public string MealName { get; set; } = string.Empty;
        public int DislikeCount { get; set; }
    }

    public class AtRiskSubscriptionDto
    {
        public int SubscriptionId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public DateOnly? EndDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class FailedPaymentDto
    {
        public int PaymentId { get; set; }
        public string PaymentCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class TopAllergyDto
    {
        public string AllergyName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}
