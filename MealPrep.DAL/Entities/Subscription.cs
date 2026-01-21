using MealPrep.DAL.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.DAL.Entities
{
    public class Subscription
    {
        public int Id { get; set; }

        [Required]
        public Guid AppUserId { get; set; }
        public AppUser? AppUser { get; set; }

        [Required, StringLength(80)]
        public string CustomerName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(120)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        public SubscriptionPlan Plan { get; set; }

        [Range(1, 10)]
        public int MealsPerDay { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
