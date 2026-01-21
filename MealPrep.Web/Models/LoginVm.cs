using System.ComponentModel.DataAnnotations;

namespace MealPrep.Web.Models
{

    public class LoginVm
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}
