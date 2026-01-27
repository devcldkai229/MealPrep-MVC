using MealPrep.DAL.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace MealPrep.DAL.Entities
{
    /// <summary>
    /// Đơn giao hàng cho một ngày cụ thể trong Subscription
    /// Mỗi Subscription sẽ có nhiều DeliveryOrder (một order mỗi ngày)
    /// </summary>
    public class DeliveryOrder
    {
        public int Id { get; set; }

        [Required]
        public int SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }

        /// <summary>
        /// Shipper được gán để giao đơn này (AppUser với Role = Shipper)
        /// </summary>
        public Guid? ShipperId { get; set; }
        public AppUser? Shipper { get; set; }

        [Required]
        public DateOnly DeliveryDate { get; set; }

        /// <summary>
        /// Trạng thái: Planned, Preparing, Delivering, Delivered, Skipped, Cancelled
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Planned;

        /// <summary>
        /// Tổng tiền của đơn hàng này (snapshot để tránh thay đổi giá ảnh hưởng)
        /// </summary>
        [Range(0, 100000000)]
        public decimal TotalAmount { get; set; }

        [StringLength(1000)]
        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<DeliveryOrderItem> Items { get; set; } = new List<DeliveryOrderItem>();
    }
}
