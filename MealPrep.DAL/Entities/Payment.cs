using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.DAL.Entities
{
    /// <summary>
    /// Thanh toán cho subscription
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public Guid AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        [Required]
        public int SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }

        /// <summary>
        /// Số tiền thanh toán
        /// </summary>
        [Range(0, 100000000)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Đơn vị tiền tệ: VND, USD, etc.
        /// </summary>
        [Required, StringLength(10)]
        public string Currency { get; set; } = "VND";

        /// <summary>
        /// Phương thức: MoMo, VNPay, BankTransfer, COD
        /// </summary>
        [Required, StringLength(50)]
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái: Pending, Paid, Failed, Cancelled, Expired
        /// </summary>
        [Required, StringLength(50)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Mã thanh toán nội bộ (unique)
        /// </summary>
        [Required, StringLength(100)]
        public string PaymentCode { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả thanh toán
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        public DateTime? ExpiredAt { get; set; }

        // Navigation
        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    }
}
