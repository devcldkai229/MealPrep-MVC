using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.DAL.Entities
{
    public class WeeklyMenu
    {
        public int Id { get; set; }

        public Guid? CreatedByUserId { get; set; }
        public AppUser? CreatedBy { get; set; }

        [Required]
        public DateOnly WeekStart { get; set; }  // Monday

        [Required]
        public DateOnly WeekEnd { get; set; }    // Sunday

        public ICollection<WeeklyMenuItem> Items { get; set; } = new List<WeeklyMenuItem>();
    }
}
