using MealPrep.DAL.Enums;
using System.ComponentModel.DataAnnotations;

namespace MealPrep.Web.ViewModels
{
    public class CompleteProfileVm
    {

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tuổi")]
        [Range(5, 100, ErrorMessage = "Tuổi phải từ 5 đến 100")]
        [Display(Name = "Tuổi")]
        public int Age { get; set; }

        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại (tùy chọn)")]
        public string? PhoneNumber { get; set; }
    }
}
