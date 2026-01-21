using System.ComponentModel.DataAnnotations;

namespace MealPrep.Web.ViewModels
{

    public class LoginVm
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}
