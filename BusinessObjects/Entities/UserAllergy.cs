using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Entities
{
    public class UserAllergy
    {
        public int Id { get; set; }

        public int UserNutritionProfileId { get; set; }
        public UserNutritionProfile UserNutritionProfile { get; set; } = null!;

        // ví d?: "Peanut", "Seafood", "Egg", "Milk", "Soy"
        [Required, StringLength(50)]
        public string AllergyName { get; set; } = string.Empty;
    }
}
