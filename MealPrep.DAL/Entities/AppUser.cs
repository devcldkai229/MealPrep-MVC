using MealPrep.DAL.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MealPrep.DAL.Entities;

public class AppUser
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public Gender Gender { get; set; } = Gender.Unknown;

    [Range(5, 100)]
    public int Age { get; set; }

    [StringLength(500)]
    public string AvatarUrl { get; set; } = string.Empty;

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Địa chỉ giao hàng của khách hàng
    /// Ví dụ: "Số 10, Đường Nguyễn Huệ, Quận 1, TP.HCM"
    /// </summary>
    [StringLength(500)]
    public string? DeliveryAddress { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAtUtc { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Guid RoleId { get; set; }
    public AppRole Role { get; set; } = null!;

    public UserNutritionProfile? NutritionProfile { get; set; }

    public ICollection<UserDislikedMeal> DislikedMeals { get; set; } = new List<UserDislikedMeal>();
    
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    
    public ICollection<NutritionLog> NutritionLogs { get; set; } = new List<NutritionLog>();
    
    public ICollection<WeeklyMenu> WeeklyMenus { get; set; } = new List<WeeklyMenu>();
}
