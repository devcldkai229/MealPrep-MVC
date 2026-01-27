using MealPrep.BLL.Exceptions;
using MealPrep.DAL.Data;
using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MealPrep.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpsertNutritionProfileAsync(
            Guid userId,
            int heightCm,
            decimal weightKg,
            FitnessGoal goal,
            ActivityLevel activityLevel,
            DietPreference dietPreference,
            int mealsPerDay,
            int? caloriesInDay,
            string? notes,
            List<string>? allergies = null)
        {
            var user = await _context.Users
                .Include(u => u.NutritionProfile!)
                    .ThenInclude(np => np.Allergies)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            // Nếu chưa có profile thì tạo mới
            if (user.NutritionProfile == null)
            {
                user.NutritionProfile = new UserNutritionProfile
                {
                    AppUserId = userId,
                    HeightCm = heightCm,
                    WeightKg = weightKg,
                    Goal = goal,
                    ActivityLevel = activityLevel,
                    DietPreference = dietPreference,
                    MealsPerDay = mealsPerDay,
                    CaloriesInDay = caloriesInDay,
                    Notes = notes
                };

                _context.Set<UserNutritionProfile>().Add(user.NutritionProfile);
            }
            else
            {
                // Cập nhật profile hiện có
                user.NutritionProfile.HeightCm = heightCm;
                user.NutritionProfile.WeightKg = weightKg;
                user.NutritionProfile.Goal = goal;
                user.NutritionProfile.ActivityLevel = activityLevel;
                user.NutritionProfile.DietPreference = dietPreference;
                user.NutritionProfile.MealsPerDay = mealsPerDay;
                user.NutritionProfile.CaloriesInDay = caloriesInDay;
                user.NutritionProfile.Notes = notes;
            }

            // Xử lý allergies nếu có
            if (allergies != null)
            {
                // Xóa allergies cũ không có trong danh sách mới
                var existingAllergies = user.NutritionProfile.Allergies?.ToList() ?? new List<UserAllergy>();
                var allergiesToRemove = existingAllergies
                    .Where(a => !allergies.Contains(a.AllergyName, StringComparer.OrdinalIgnoreCase))
                    .ToList();
                
                foreach (var allergy in allergiesToRemove)
                {
                    _context.Set<UserAllergy>().Remove(allergy);
                }

                // Thêm allergies mới
                var existingAllergyNames = existingAllergies
                    .Select(a => a.AllergyName)
                    .ToList();
                
                foreach (var allergyName in allergies)
                {
                    if (!string.IsNullOrWhiteSpace(allergyName) && 
                        !existingAllergyNames.Contains(allergyName, StringComparer.OrdinalIgnoreCase))
                    {
                        var newAllergy = new UserAllergy
                        {
                            UserNutritionProfileId = user.NutritionProfile.Id,
                            AllergyName = allergyName.Trim()
                        };
                        _context.Set<UserAllergy>().Add(newAllergy);
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task AddDislikedMealAsync(Guid userId, int mealId)
        {
            var existing = await _context.Set<UserDislikedMeal>()
                .FirstOrDefaultAsync(dm => dm.AppUserId == userId && dm.MealId == mealId);
            
            if (existing == null)
            {
                var dislikedMeal = new UserDislikedMeal
                {
                    AppUserId = userId,
                    MealId = mealId
                };
                _context.Set<UserDislikedMeal>().Add(dislikedMeal);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveDislikedMealAsync(Guid userId, int mealId)
        {
            var dislikedMeal = await _context.Set<UserDislikedMeal>()
                .FirstOrDefaultAsync(dm => dm.AppUserId == userId && dm.MealId == mealId);
            
            if (dislikedMeal != null)
            {
                _context.Set<UserDislikedMeal>().Remove(dislikedMeal);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsMealDislikedAsync(Guid userId, int mealId)
        {
            return await _context.Set<UserDislikedMeal>()
                .AnyAsync(dm => dm.AppUserId == userId && dm.MealId == mealId);
        }
        public async Task<AuthResponse> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            return new AuthResponse(
                UserId: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                RoleId: user.RoleId,
                RoleName: user.Role.Name,
                IsActive: user.IsActive,
                AvatarUrl: user.AvatarUrl
            );
        }

        public async Task<DAL.Entities.AppUser?> GetUserProfileAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.NutritionProfile!)
                    .ThenInclude(np => np.Allergies)
                .Include(u => u.DislikedMeals)
                    .ThenInclude(dm => dm.Meal)
                .Include(u => u.Subscriptions)
                .Include(u => u.Orders)
                .Include(u => u.NutritionLogs)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task UpdateProfileAsync(Guid userId, string fullName, string? phoneNumber, Gender gender, int age, string? avatarUrl)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            user.FullName = fullName;
            user.PhoneNumber = phoneNumber;
            user.Gender = gender;
            user.Age = age;
            user.AvatarUrl = avatarUrl ?? string.Empty;

            await _context.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                throw new InvalidPasswordException();
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task DeactivateAccountAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            user.IsActive = false;
            await _context.SaveChangesAsync();
        }

        public async Task ActivateAccountAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new UserNotFoundException(userId);
            }

            user.IsActive = true;
            await _context.SaveChangesAsync();
        }

        public async Task<List<string>> GetUserAllergiesAsync(Guid userId)
        {
            return await _context.Set<UserAllergy>()
                .Include(a => a.UserNutritionProfile)
                .Where(a => a.UserNutritionProfile!.AppUserId == userId)
                .Select(a => a.AllergyName.ToLower())
                .ToListAsync();
        }
    }
}
