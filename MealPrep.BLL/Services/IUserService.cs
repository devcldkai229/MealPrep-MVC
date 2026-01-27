using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using System;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public interface IUserService
    {
        /// <summary>
        /// Lấy thông tin user theo ID
        /// </summary>
        /// <exception cref="Exceptions.UserNotFoundException">User không tồn tại</exception>
        Task<AuthResponse> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Lấy thông tin user đầy đủ cho Profile page (bao gồm navigation properties)
        /// </summary>
        Task<AppUser?> GetUserProfileAsync(Guid userId);

        /// <summary>
        /// Cập nhật thông tin profile
        /// </summary>
        /// <exception cref="Exceptions.UserNotFoundException">User không tồn tại</exception>
        Task UpdateProfileAsync(Guid userId, string fullName, string? phoneNumber, Gender gender, int age, string? avatarUrl);

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        /// <exception cref="Exceptions.UserNotFoundException">User không tồn tại</exception>
        /// <exception cref="Exceptions.InvalidPasswordException">Mật khẩu hiện tại không đúng</exception>
        Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

        /// <summary>
        /// Kiểm tra email đã tồn tại chưa
        /// </summary>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Vô hiệu hóa tài khoản
        /// </summary>
        /// <exception cref="Exceptions.UserNotFoundException">User không tồn tại</exception>
        Task DeactivateAccountAsync(Guid userId);

        /// <summary>
        /// Kích hoạt lại tài khoản
        /// </summary>
        /// <exception cref="Exceptions.UserNotFoundException">User không tồn tại</exception>
        Task ActivateAccountAsync(Guid userId);
        /// <summary>
        /// Tạo hoặc cập nhật hồ sơ dinh dưỡng
        /// </summary>
        Task UpsertNutritionProfileAsync(
            Guid userId,
            int heightCm,
            decimal weightKg,
            FitnessGoal goal,
            ActivityLevel activityLevel,
            DietPreference dietPreference,
            int mealsPerDay,
            int? caloriesInDay,
            string? notes,
            List<string>? allergies = null
        );

        /// <summary>
        /// Thêm món không thích
        /// </summary>
        Task AddDislikedMealAsync(Guid userId, int mealId);

        /// <summary>
        /// Xóa món không thích
        /// </summary>
        Task RemoveDislikedMealAsync(Guid userId, int mealId);

        /// <summary>
        /// Kiểm tra user có không thích món này không
        /// </summary>
        Task<bool> IsMealDislikedAsync(Guid userId, int mealId);

        /// <summary>
        /// Lấy danh sách allergies của user (lowercase)
        /// </summary>
        Task<List<string>> GetUserAllergiesAsync(Guid userId);
    }
}
