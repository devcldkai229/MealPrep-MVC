using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace MealPrep.DAL.Data
{
    public static class SeedData
    {
        public static void SeedAll(ModelBuilder modelBuilder)
        {
            var seedDate = new DateTime(2026, 1, 21, 0, 0, 0, DateTimeKind.Utc);
            var mealsSeedDate = new DateTime(2026, 1, 24, 0, 0, 0, DateTimeKind.Utc);

            SeedRoles(modelBuilder);
            SeedPlans(modelBuilder, seedDate);
            SeedPlanMealTiers(modelBuilder, seedDate);
            SeedMeals(modelBuilder, mealsSeedDate);
            SeedUsers(modelBuilder, seedDate);
            SeedUserNutritionProfiles(modelBuilder);
            SeedUserAllergies(modelBuilder);
            SeedDeliverySlots(modelBuilder, seedDate);
        }

        private static void SeedRoles(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppRole>().HasData(
                new AppRole
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Admin"
                },
                new AppRole
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "User"
                }
            );
        }

        private static void SeedPlans(ModelBuilder modelBuilder, DateTime seedDate)
        {
            modelBuilder.Entity<Plan>().HasData(
                new Plan
                {
                    Id = 1,
                    Name = "Weekly",
                    Description = "Gói đăng ký theo tuần (7 ngày)",
                    DurationDays = 7,
                    BasePrice = 300000,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new Plan
                {
                    Id = 2,
                    Name = "Monthly",
                    Description = "Gói đăng ký theo tháng (30 ngày)",
                    DurationDays = 30,
                    BasePrice = 1000000,
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );
        }

        private static void SeedPlanMealTiers(ModelBuilder modelBuilder, DateTime seedDate)
        {
            modelBuilder.Entity<PlanMealTier>().HasData(
                new PlanMealTier { Id = 1, PlanId = 1, MealsPerDay = 1, ExtraPrice = 0, IsActive = true, CreatedAt = seedDate },
                new PlanMealTier { Id = 2, PlanId = 1, MealsPerDay = 2, ExtraPrice = 150000, IsActive = true, CreatedAt = seedDate },
                new PlanMealTier { Id = 3, PlanId = 1, MealsPerDay = 3, ExtraPrice = 280000, IsActive = true, CreatedAt = seedDate },
                new PlanMealTier { Id = 4, PlanId = 2, MealsPerDay = 1, ExtraPrice = 0, IsActive = true, CreatedAt = seedDate },
                new PlanMealTier { Id = 5, PlanId = 2, MealsPerDay = 2, ExtraPrice = 500000, IsActive = true, CreatedAt = seedDate },
                new PlanMealTier { Id = 6, PlanId = 2, MealsPerDay = 3, ExtraPrice = 900000, IsActive = true, CreatedAt = seedDate }
            );
        }

        private static void SeedMeals(ModelBuilder modelBuilder, DateTime mealsSeedDate)
        {
            var meals = MealSeedData.GetAllMeals(mealsSeedDate);
            modelBuilder.Entity<Meal>().HasData(meals);
        }

        private static void SeedUsers(ModelBuilder modelBuilder, DateTime seedDate)
        {
            var adminId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var userId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            // Password hashes (hardcoded for consistency in migrations)
            // Admin: admin@mealprep.com / Admin123!
            // User: user@mealprep.com / User123!
            var adminPasswordHash = "$2a$11$BgEVIOqTroolit.QXo8ZVedAuTsoAevkQVuyc/AhK02D9iDFQvimu";
            var userPasswordHash = "$2a$11$w6EYO9JNYHx7EsbDMmv85.9akoeiDFVXsAmfmTn3BXtY1mueukRcG";

            modelBuilder.Entity<AppUser>().HasData(
                new AppUser
                {
                    Id = adminId,
                    Email = "admin@mealprep.com",
                    FullName = "Administrator",
                    PasswordHash = adminPasswordHash,
                    Gender = Gender.Male,
                    Age = 30,
                    PhoneNumber = "0901234567",
                    RoleId = adminRoleId,
                    IsActive = true,
                    CreatedAtUtc = seedDate
                },
                new AppUser
                {
                    Id = userId,
                    Email = "user@mealprep.com",
                    FullName = "Nguyễn Văn A",
                    PasswordHash = userPasswordHash,
                    Gender = Gender.Male,
                    Age = 28,
                    PhoneNumber = "0907654321",
                    RoleId = userRoleId,
                    IsActive = true,
                    CreatedAtUtc = seedDate
                }
            );
        }

        private static void SeedUserNutritionProfiles(ModelBuilder modelBuilder)
        {
            var userId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            modelBuilder.Entity<UserNutritionProfile>().HasData(
                new UserNutritionProfile
                {
                    Id = 1,
                    AppUserId = userId,
                    HeightCm = 175,
                    WeightKg = 75.5m,
                    Goal = FitnessGoal.MuscleGain,
                    ActivityLevel = ActivityLevel.ModeratelyActive,
                    DietPreference = DietPreference.None,
                    MealsPerDay = 3,
                    Notes = "Tập gym 3-4 lần/tuần, muốn tăng cơ, thích ăn ngọt nhiều và cay ít"
                }
            );
        }

        private static void SeedUserAllergies(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAllergy>().HasData(
                new UserAllergy
                {
                    Id = 1,
                    UserNutritionProfileId = 1,
                    AllergyName = "Đậu phộng"
                },
                new UserAllergy
                {
                    Id = 2,
                    UserNutritionProfileId = 1,
                    AllergyName = "Hải sản"
                }
            );
        }

        private static void SeedDeliverySlots(ModelBuilder modelBuilder, DateTime seedDate)
        {
            modelBuilder.Entity<DeliverySlot>().HasData(
                new DeliverySlot
                {
                    Id = 1,
                    Name = "Morning",
                    Capacity = 100,
                    IsActive = true
                },
                new DeliverySlot
                {
                    Id = 2,
                    Name = "Afternoon",
                    Capacity = 100,
                    IsActive = false // Disabled - chỉ dùng Morning và Evening
                },
                new DeliverySlot
                {
                    Id = 3,
                    Name = "Evening",
                    Capacity = 100,
                    IsActive = true
                }
            );
        }
    }
}
