using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    /// <summary>
    /// H?p d?ng dang ký gói meal prep
    /// </summary>
    public class Subscription
    {
        public int Id { get; set; }

        [Required]
        public Guid AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        [Required]
        public int PlanId { get; set; }
        public Plan? Plan { get; set; }

        [Required, StringLength(80)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(120)]
        public string CustomerEmail { get; set; } = string.Empty;

        /// <summary>
        /// S? b?a an m?i ngày: 1, 2, ho?c 3
        /// </summary>
        [Range(1, 3)]
        public int MealsPerDay { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        /// <summary>
        /// Ngày k?t thúc (tính t? StartDate + Plan.DurationDays)
        /// </summary>
        public DateOnly? EndDate { get; set; }

        /// <summary>
        /// Tr?ng thái: PendingPayment, Active, Paused, Cancelled, Expired
        /// </summary>
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.PendingPayment;

        /// <summary>
        /// T?ng ti?n dã snapshot khi t?o subscription
        /// = Plan.BasePrice + PlanMealTier.ExtraPrice
        /// </summary>
        [Range(0, 100000000)]
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<DeliveryOrder> DeliveryOrders { get; set; } = new List<DeliveryOrder>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
