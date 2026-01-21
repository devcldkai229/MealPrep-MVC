using System.ComponentModel.DataAnnotations;

namespace MealPrep.Web.Models
{
    public class RegisterVm
    {
        [Required, EmailAddress, StringLength(256)]
        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Required, StringLength(500)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = "";

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = "";

        [Required, Compare(nameof(Password))]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = "";

        [Required, StringLength(6, MinimumLength = 6)]
        [Display(Name = "Mã OTP")]
        public string OtpCode { get; set; } = "";
    }
}
