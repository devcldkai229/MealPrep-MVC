using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class UserDislikedMeal
    {
        public int Id { get; set; }

        public Guid AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;

        public int MealId { get; set; }
        public Meal Meal { get; set; } = null!;
    }
}
