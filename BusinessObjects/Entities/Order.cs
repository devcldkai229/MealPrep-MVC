using BusinessObjects.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public Guid AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        [Required]
        public int SubscriptionId { get; set; }
        public Subscription? Subscription { get; set; }

        [Required]
        public DateOnly DeliveryDate { get; set; }

        [Required]
        public int DeliverySlotId { get; set; }
        public DeliverySlot? DeliverySlot { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Planned;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
