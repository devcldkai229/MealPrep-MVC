using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities
{
    /// <summary>
    /// Quản lý kho nguyên liệu theo ngày cho từng món ăn
    /// Mỗi món có giới hạn số lượng có thể phục vụ trong một ngày cụ thể
    /// </summary>
    public class KitchenInventory
    {
        public int Id { get; set; }

        /// <summary>
        /// Ngày áp dụng giới hạn (chỉ lưu Date, không có Time)
        /// </summary>
        [Required]
        public DateOnly Date { get; set; }

        /// <summary>
        /// Món ăn nào
        /// </summary>
        [Required]
        public int MealId { get; set; }
        public Meal? Meal { get; set; }

        /// <summary>
        /// Giới hạn số lượng suất có thể phục vụ trong ngày này
        /// Ví dụ: 50 suất Cá hồi cho ngày 29/01/2026
        /// </summary>
        [Range(0, 10000)]
        public int QuantityLimit { get; set; }

        /// <summary>
        /// Số lượng đã được đặt (tính từ DeliveryOrderItems)
        /// Có thể tính động hoặc cache để tăng performance
        /// </summary>
        [Range(0, 10000)]
        public int QuantityUsed { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Ghi chú (tùy chọn)
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
