using System.ComponentModel.DataAnnotations;

namespace MealPrep.DAL.Entities
{
    /// <summary>
    /// Món ăn cụ thể trong một DeliveryOrder
    /// Mỗi DeliveryOrder có MealsPerDay items (1-3 items)
    /// </summary>
    public class DeliveryOrderItem
    {
        public int Id { get; set; }

        [Required]
        public int DeliveryOrderId { get; set; }
        public DeliveryOrder? DeliveryOrder { get; set; }

        /// <summary>
        /// Nullable vì có thể chưa assign meal cụ thể
        /// </summary>
        public int? MealId { get; set; }
        public Meal? Meal { get; set; }

        /// <summary>
        /// Snapshot tên món để tránh phụ thuộc vào Meal entity
        /// </summary>
        [StringLength(255)]
        public string MealNameSnapshot { get; set; } = string.Empty;

        /// <summary>
        /// Loại bữa: Breakfast, Lunch, Dinner (optional)
        /// </summary>
        [StringLength(50)]
        public string? MealType { get; set; }

        /// <summary>
        /// Số lượng (thường = 1 cho mỗi bữa)
        /// </summary>
        [Range(1, 10)]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Giá đơn vị (snapshot)
        /// </summary>
        [Range(0, 10000000)]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Địa chỉ giao hàng tại thời điểm tạo đơn (snapshot)
        /// Để shipper biết giao đến địa chỉ nào
        /// </summary>
        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        /// <summary>
        /// S3 Key của ảnh xác nhận giao hàng
        /// Shipper chụp ảnh khi giao hàng thành công và upload lên S3
        /// </summary>
        [StringLength(500)]
        public string? ImageS3Key { get; set; }

        /// <summary>
        /// Thời điểm giao hàng thành công (khi shipper bấm "Hoàn thành")
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
