using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities
{
    /// <summary>
    /// Định nghĩa gói đăng ký (Weekly/Monthly)
    /// </summary>
    public class Plan
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty; // "Weekly", "Monthly"

        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Số ngày của gói: 7 (Weekly) hoặc 30 (Monthly)
        /// </summary>
        [Range(1, 365)]
        public int DurationDays { get; set; }

        /// <summary>
        /// Giá cơ bản của gói (chưa tính số bữa/ngày)
        /// </summary>
        [Range(0, 100000000)]
        public decimal BasePrice { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<PlanMealTier> MealTiers { get; set; } = new List<PlanMealTier>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
