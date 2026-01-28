using System.ComponentModel.DataAnnotations;

namespace MealPrep.Web.ViewModels
{
    public class RegisterVm
    {
        [Required, EmailAddress, StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required, StringLength(500)]
        [Display(Name = "H? và tên")]
        public string FullName { get; set; } = "";

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        [Display(Name = "M?t kh?u")]
        public string Password { get; set; } = "";

        [Required, Compare(nameof(Password))]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nh?n m?t kh?u")]
        public string ConfirmPassword { get; set; } = "";

        [Required, StringLength(6, MinimumLength = 6)]
        [Display(Name = "Mã OTP")]
        public string OtpCode { get; set; } = "";
    }
}
