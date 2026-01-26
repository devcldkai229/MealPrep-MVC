using System.ComponentModel.DataAnnotations;

namespace MealPrep.DAL.Entities
{
    /// <summary>
    /// 📊 Đánh giá món ăn của User sau khi nhận hàng
    /// Flow 8: Meal Feedback & Preference Learning
    /// </summary>
    public class MealRating
    {
        public int Id { get; set; }

        [Required]
        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;

        [Required]
        public int DeliveryOrderItemId { get; set; }
        public DeliveryOrderItem DeliveryOrderItem { get; set; } = null!;

        [Required]
        public int MealId { get; set; }
        public Meal Meal { get; set; } = null!;

        /// <summary>
        /// Ngày giao hàng (để dễ query món của ngày hôm qua)
        /// </summary>
        [Required]
        public DateOnly DeliveryDate { get; set; }

        /// <summary>
        /// Số sao: 1-5 sao
        /// 1-2 sao: Bad (hỏi chặn món)
        /// 3 sao: OK
        /// 4-5 sao: Excellent (tăng priority)
        /// </summary>
        [Range(1, 5)]
        public int Stars { get; set; }

        /// <summary>
        /// Tags (Optional): "Hơi mặn", "Khô", "Ít đạm", "Ngon tuyệt"
        /// Lưu dạng JSON array
        /// </summary>
        [StringLength(500)]
        public string? Tags { get; set; }

        /// <summary>
        /// Ghi chú thêm (optional)
        /// </summary>
        [StringLength(1000)]
        public string? Comments { get; set; }

        /// <summary>
        /// User có muốn chặn món này vĩnh viễn không? (nếu 1-2 sao)
        /// </summary>
        public bool RequestedBlock { get; set; } = false;

        /// <summary>
        /// Đã confirm là đã "ăn" (Consumed) → Ghi vào NutritionLog
        /// </summary>
        public bool MarkedAsConsumed { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
