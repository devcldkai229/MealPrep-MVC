using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MealPrep.DAL.Entities
{
    /// <summary>
    /// Món ăn trong hệ thống
    /// </summary>
    public class Meal
    {
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public string Ingredients { get; set; } // JSON array of strings

        public string Images { get;set; } // JSON array of image URLs

        [StringLength(10000)]
        public string? Description { get; set; }

        [Range(0, 10000)]                                                    
        public int Calories { get; set; }

        [Range(0, 10000)]
        public decimal Protein { get; set; }  // grams

        [Range(0, 10000)]
        public decimal Carbs { get; set; }

        [Range(0, 10000)]
        public decimal Fat { get; set; }

        /// <summary>
        /// Giá cơ bản của món (nếu bán lẻ)
        /// </summary>
        [Range(0, 10000000)]
        public decimal BasePrice { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
