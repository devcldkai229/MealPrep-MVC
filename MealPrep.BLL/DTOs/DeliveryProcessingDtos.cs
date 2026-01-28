using BusinessObjects.Enums;

namespace MealPrep.BLL.DTOs
{
    /// <summary>
    /// DTO cho Kitchen List - Danh sách món cần nấu
    /// </summary>
    public record KitchenListItemDto(
        int MealId,
        string MealName,
        int TotalQuantity, // Tổng số suất cần nấu
        decimal UnitPrice,
        int Calories,
        string Ingredients
    );

    /// <summary>
    /// DTO cho Kitchen Export - Export full report
    /// </summary>
    public class KitchenExportDto
    {
        public DateOnly DeliveryDate { get; set; }
        public int TotalDeliveryOrders { get; set; }
        public int TotalMealPortions { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<KitchenListItemDto> Items { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// DTO cho Delivery Order với customer info
    /// </summary>
    public record DeliveryOrderDetailDto(
        int Id,
        int SubscriptionId,
        Guid CustomerId,
        string CustomerName,
        string CustomerEmail,
        string? CustomerPhone,
        DateOnly DeliveryDate,
        string? DeliverySlotName,
        OrderStatus Status,
        decimal TotalAmount,
        List<DeliveryOrderItemDto> Items
    );

    public record DeliveryOrderItemDto(
        int Id,
        int? MealId,
        string MealName,
        string? MealType,
        int Quantity,
        decimal UnitPrice
    );

    /// <summary>
    /// Result khi generate delivery orders
    /// </summary>
    public class GenerateDeliveryOrdersResult
    {
        public int TotalSubscriptionsProcessed { get; set; }
        public int TotalOrdersCreated { get; set; }
        public int TotalAutoAssignedMeals { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}
