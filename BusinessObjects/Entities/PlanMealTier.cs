using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities
{
    /// <summary>
    /// Bảng giá theo số bữa ăn/ngày cho từng Plan
    /// Ví dụ: Plan Weekly với 1 meal/day có ExtraPrice = 0
    ///        Plan Weekly với 2 meals/day có ExtraPrice = 50000
    ///        Plan Weekly với 3 meals/day có ExtraPrice = 100000
    /// </summary>
    public class PlanMealTier
    {
        public int Id { get; set; }

        [Required]
        public int PlanId { get; set; }
        public Plan? Plan { get; set; }

        /// <summary>
        /// Số bữa ăn mỗi ngày: 1, 2, hoặc 3
        /// </summary>
        [Range(1, 3)]
        public int MealsPerDay { get; set; }

        /// <summary>
        /// Giá thêm so với BasePrice của Plan
        /// Giá cuối cùng = Plan.BasePrice + PlanMealTier.ExtraPrice
        /// </summary>
        [Range(0, 100000000)]
        public decimal ExtraPrice { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
