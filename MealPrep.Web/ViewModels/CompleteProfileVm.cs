using BusinessObjects.Enums;
using System.ComponentModel.DataAnnotations;

namespace MealPrep.Web.ViewModels
{
    public class CompleteProfileVm
    {

        [Required(ErrorMessage = "Vui lòng ch?n gi?i tính")]
        [Display(Name = "Gi?i tính")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Vui lòng nh?p tu?i")]
        [Range(5, 100, ErrorMessage = "Tu?i ph?i t? 5 d?n 100")]
        [Display(Name = "Tu?i")]
        public int Age { get; set; }

        [StringLength(20, ErrorMessage = "S? di?n tho?i không du?c vu?t quá 20 ký t?")]
        [Phone(ErrorMessage = "S? di?n tho?i không h?p l?")]
        [Display(Name = "S? di?n tho?i (tùy ch?n)")]
        public string? PhoneNumber { get; set; }
    }
}
