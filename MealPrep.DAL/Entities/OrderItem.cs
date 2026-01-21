using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.DAL.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Required]
        public int MealId { get; set; }
        public Meal? Meal { get; set; }

        [Range(1, 10)]
        public int Quantity { get; set; } = 1;
    }
}
