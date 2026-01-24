using MealPrep.DAL.Entities;
using MealPrep.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealPrep.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<AppRole> Roles => Set<AppRole>();
        public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
        public DbSet<Meal> Meals => Set<Meal>();
        public DbSet<WeeklyMenu> WeeklyMenus => Set<WeeklyMenu>();
        public DbSet<WeeklyMenuItem> WeeklyMenuItems => Set<WeeklyMenuItem>();
        
        // New pricing entities
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<PlanMealTier> PlanMealTiers => Set<PlanMealTier>();
        
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        
        // New delivery entities
        public DbSet<DeliveryOrder> DeliveryOrders => Set<DeliveryOrder>();
        public DbSet<DeliveryOrderItem> DeliveryOrderItems => Set<DeliveryOrderItem>();
        public DbSet<DeliverySlot> DeliverySlots => Set<DeliverySlot>();
        
        // New payment entities
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
        
        public DbSet<NutritionLog> NutritionLogs => Set<NutritionLog>();
        public DbSet<UserNutritionProfile> UserNutritionProfiles => Set<UserNutritionProfile>();
        public DbSet<UserAllergy> UserAllergies => Set<UserAllergy>();
        public DbSet<UserDislikedMeal> UserDislikedMeals => Set<UserDislikedMeal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // AppUser
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.FullName)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(u => u.CreatedAtUtc)
                    .HasDefaultValueSql("GETUTCDATE()");

                // User N-1 với Role
                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Users)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // UserNutritionProfile 1-1 với AppUser
            modelBuilder.Entity<UserNutritionProfile>(entity =>
            {
                entity.HasOne(p => p.AppUser)
                    .WithOne(u => u.NutritionProfile!)
                    .HasForeignKey<UserNutritionProfile>(p => p.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(p => p.WeightKg)
                    .HasPrecision(5, 2);
            });

            // UserAllergy 1-n với UserNutritionProfile
            modelBuilder.Entity<UserAllergy>(entity =>
            {
                entity.HasOne(a => a.UserNutritionProfile)
                    .WithMany(p => p.Allergies)
                    .HasForeignKey(a => a.UserNutritionProfileId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(a => a.AllergyName)
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // UserDislikedMeal n-m giữa AppUser và Meal (join entity)
            modelBuilder.Entity<UserDislikedMeal>(entity =>
            {
                entity.HasIndex(x => new { x.AppUserId, x.MealId })
                    .IsUnique();

                entity.HasOne(x => x.AppUser)
                    .WithMany(u => u.DislikedMeals)
                    .HasForeignKey(x => x.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Meal)
                    .WithMany()
                    .HasForeignKey(x => x.MealId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Meal nutrition precision
            modelBuilder.Entity<Meal>(entity =>
            {
                entity.Property(m => m.Protein).HasPrecision(5, 2);
                entity.Property(m => m.Carbs).HasPrecision(5, 2);
                entity.Property(m => m.Fat).HasPrecision(5, 2);
                entity.Property(m => m.BasePrice).HasPrecision(10, 2);
            });

            // Plan
            modelBuilder.Entity<Plan>(entity =>
            {
                entity.Property(p => p.BasePrice).HasPrecision(10, 2);
                entity.HasIndex(p => p.Name).IsUnique();
            });

            // PlanMealTier - Unique constraint
            modelBuilder.Entity<PlanMealTier>(entity =>
            {
                entity.Property(t => t.ExtraPrice).HasPrecision(10, 2);
                
                // Unique: PlanId + MealsPerDay
                entity.HasIndex(t => new { t.PlanId, t.MealsPerDay }).IsUnique();

                entity.HasOne(t => t.Plan)
                    .WithMany(p => p.MealTiers)
                    .HasForeignKey(t => t.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Subscription
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.Property(s => s.TotalAmount).HasPrecision(10, 2);

                entity.HasOne(s => s.AppUser)
                    .WithMany(u => u.Subscriptions)
                    .HasForeignKey(s => s.AppUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Plan)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(s => s.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.Property(p => p.Amount).HasPrecision(10, 2);
                
                // Unique payment code
                entity.HasIndex(p => p.PaymentCode).IsUnique();

                entity.HasOne(p => p.AppUser)
                    .WithMany()
                    .HasForeignKey(p => p.AppUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Subscription)
                    .WithMany(s => s.Payments)
                    .HasForeignKey(p => p.SubscriptionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // PaymentTransaction
            modelBuilder.Entity<PaymentTransaction>(entity =>
            {
                entity.HasOne(t => t.Payment)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(t => t.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // DeliveryOrder
            modelBuilder.Entity<DeliveryOrder>(entity =>
            {
                entity.Property(d => d.TotalAmount).HasPrecision(10, 2);

                entity.HasOne(d => d.Subscription)
                    .WithMany(s => s.DeliveryOrders)
                    .HasForeignKey(d => d.SubscriptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.DeliverySlot)
                    .WithMany()
                    .HasForeignKey(d => d.DeliverySlotId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // DeliveryOrderItem
            modelBuilder.Entity<DeliveryOrderItem>(entity =>
            {
                entity.Property(i => i.UnitPrice).HasPrecision(10, 2);

                entity.HasOne(i => i.DeliveryOrder)
                    .WithMany(d => d.Items)
                    .HasForeignKey(i => i.DeliveryOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Meal)
                    .WithMany()
                    .HasForeignKey(i => i.MealId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Order N-1 với AppUser
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.AppUser)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.AppUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // NutritionLog N-1 với AppUser
            modelBuilder.Entity<NutritionLog>(entity =>
            {
                entity.HasOne(n => n.AppUser)
                    .WithMany(u => u.NutritionLogs)
                    .HasForeignKey(n => n.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // WeeklyMenu N-1 với AppUser (CreatedBy)
            modelBuilder.Entity<WeeklyMenu>(entity =>
            {
                entity.HasOne(w => w.CreatedBy)
                    .WithMany(u => u.WeeklyMenus)
                    .HasForeignKey(w => w.CreatedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // WeeklyMenuItem relations
            modelBuilder.Entity<WeeklyMenuItem>()
                .HasOne(x => x.WeeklyMenu)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.WeeklyMenuId);

            // OrderItem relations
            modelBuilder.Entity<OrderItem>()
                .HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId);

            // Seed roles
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

            // Seed Plans
            var seedDate = new DateTime(2026, 1, 21, 0, 0, 0, DateTimeKind.Utc);
            
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

            // Seed PlanMealTiers
            modelBuilder.Entity<PlanMealTier>().HasData(
                // Weekly plan tiers
                new PlanMealTier { Id = 1, PlanId = 1, MealsPerDay = 1, ExtraPrice = 0, IsActive = true, CreatedAt = seedDate },
                new PlanMealTier { Id = 2, PlanId = 1, MealsPerDay = 2, ExtraPrice = 150000, IsActive = true, CreatedAt = seedDate },
                new PlanMealTier { Id = 3, PlanId = 1, MealsPerDay = 3, ExtraPrice = 280000, IsActive = true, CreatedAt = seedDate },
                
                // Monthly plan tiers
                new PlanMealTier { Id = 4, PlanId = 2, MealsPerDay = 1, ExtraPrice = 0, IsActive = true, CreatedAt = seedDate },
                new PlanMealTier { Id = 5, PlanId = 2, MealsPerDay = 2, ExtraPrice = 500000, IsActive = true, CreatedAt = seedDate },
                new PlanMealTier { Id = 6, PlanId = 2, MealsPerDay = 3, ExtraPrice = 900000, IsActive = true, CreatedAt = seedDate }
            );

            // Seed Meals
            var mealsSeedDate = new DateTime(2026, 1, 24, 0, 0, 0, DateTimeKind.Utc);
            modelBuilder.Entity<Meal>().HasData(
                new Meal
                {
                    Id = 1,
                    Name = "Gà Nướng Mật Ong",
                    Ingredients = "[\"Ức gà\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Hành tây\",\"Dầu oliu\",\"Muối\",\"Tiêu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\",\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Ức gà nướng thơm lừng với sốt mật ong đậm đà, kèm rau củ tươi ngon. Món ăn giàu protein, ít calo, phù hợp cho chế độ ăn kiêng và tập luyện.",
                    Calories = 320,
                    Protein = 35.5m,
                    Carbs = 18.2m,
                    Fat = 12.8m,
                    BasePrice = 85000,
                    IsActive = true,
                    CreatedAt = mealsSeedDate
                },
                new Meal
                {
                    Id = 2,
                    Name = "Cá Hồi Áp Chảo",
                    Ingredients = "[\"Cá hồi\",\"Bơ\",\"Chanh\",\"Thì là\",\"Khoai tây\",\"Bông cải xanh\",\"Muối\",\"Tiêu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\",\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá hồi tươi ngon áp chảo với lớp da giòn tan, kèm rau củ hấp. Nguồn Omega-3 dồi dào, tốt cho tim mạch và não bộ.",
                    Calories = 450,
                    Protein = 38.0m,
                    Carbs = 25.0m,
                    Fat = 22.5m,
                    BasePrice = 120000,
                    IsActive = true,
                    CreatedAt = mealsSeedDate
                },
                new Meal
                {
                    Id = 3,
                    Name = "Salad Gà Nướng",
                    Ingredients = "[\"Ức gà\",\"Xà lách\",\"Cà chua bi\",\"Dưa chuột\",\"Ớt chuông\",\"Dầu giấm\",\"Phô mai feta\",\"Hạt óc chó\"]",
                    Images = "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\",\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]",
                    Description = "Salad tươi ngon với ức gà nướng, rau xanh giòn, phô mai feta và hạt óc chó. Món ăn nhẹ, giàu chất xơ và vitamin.",
                    Calories = 280,
                    Protein = 28.0m,
                    Carbs = 15.0m,
                    Fat = 14.0m,
                    BasePrice = 75000,
                    IsActive = true,
                    CreatedAt = mealsSeedDate
                },
                new Meal
                {
                    Id = 4,
                    Name = "Bò Xào Rau Củ",
                    Ingredients = "[\"Thịt bò\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Nấm\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Dầu mè\"]",
                    Images = "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\",\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]",
                    Description = "Thịt bò mềm xào với rau củ tươi, sốt đậm đà. Món ăn giàu sắt và protein, phù hợp cho người tập gym.",
                    Calories = 380,
                    Protein = 32.0m,
                    Carbs = 22.0m,
                    Fat = 18.5m,
                    BasePrice = 95000,
                    IsActive = true,
                    CreatedAt = mealsSeedDate
                },
                new Meal
                {
                    Id = 5,
                    Name = "Cơm Gà Tây Nướng",
                    Ingredients = "[\"Gà tây\",\"Gạo lứt\",\"Đậu xanh\",\"Cà rốt\",\"Hành tây\",\"Tỏi\",\"Gia vị nướng\",\"Dầu oliu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\",\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Gà tây nướng thơm lừng với cơm gạo lứt và đậu xanh. Món ăn cân bằng dinh dưỡng, giàu protein và chất xơ.",
                    Calories = 420,
                    Protein = 40.0m,
                    Carbs = 45.0m,
                    Fat = 10.0m,
                    BasePrice = 88000,
                    IsActive = true,
                    CreatedAt = mealsSeedDate
                },
                new Meal
                {
                    Id = 6,
                    Name = "Tôm Sốt Cam",
                    Ingredients = "[\"Tôm tươi\",\"Cam\",\"Mật ong\",\"Gừng\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Dầu oliu\",\"Muối\"]",
                    Images = "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\",\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]",
                    Description = "Tôm tươi sốt cam chua ngọt, kèm rau củ. Món ăn giàu protein, ít calo, phù hợp cho chế độ ăn kiêng.",
                    Calories = 250,
                    Protein = 30.0m,
                    Carbs = 20.0m,
                    Fat = 8.0m,
                    BasePrice = 110000,
                    IsActive = true,
                    CreatedAt = mealsSeedDate
                },
                new Meal
                {
                    Id = 7,
                    Name = "Cháo Yến Mạch Thịt Bò",
                    Ingredients = "[\"Yến mạch\",\"Thịt bò băm\",\"Hành tây\",\"Cà rốt\",\"Nấm\",\"Hành lá\",\"Gừng\",\"Nước dùng\",\"Muối\"]",
                    Images = "[\"https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=800\",\"https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=800\"]",
                    Description = "Cháo yến mạch ấm nóng với thịt bò băm nhuyễn, rau củ. Món ăn dễ tiêu, giàu chất xơ và protein.",
                    Calories = 320,
                    Protein = 25.0m,
                    Carbs = 38.0m,
                    Fat = 9.5m,
                    BasePrice = 70000,
                    IsActive = true,
                    CreatedAt = mealsSeedDate
                },
                new Meal
                {
                    Id = 8,
                    Name = "Ức Gà Sốt Teriyaki",
                    Ingredients = "[\"Ức gà\",\"Nước tương\",\"Mật ong\",\"Gừng\",\"Tỏi\",\"Hành tây\",\"Ớt chuông\",\"Dầu mè\",\"Hạt mè\"]",
                    Images = "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\",\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]",
                    Description = "Ức gà sốt teriyaki đậm đà kiểu Nhật, kèm rau củ xào. Món ăn giàu protein, ít béo, phù hợp cho người tập luyện.",
                    Calories = 350,
                    Protein = 38.0m,
                    Carbs = 28.0m,
                    Fat = 10.5m,
                    BasePrice = 90000,
                    IsActive = true,
                    CreatedAt = mealsSeedDate
                },
                new Meal
                {
                    Id = 9,
                    Name = "Cá Ngừ Nướng",
                    Ingredients = "[\"Cá ngừ\",\"Chanh\",\"Tỏi\",\"Thì là\",\"Khoai lang\",\"Bông cải xanh\",\"Dầu oliu\",\"Muối\",\"Tiêu\"]",
                    Images = "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\",\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]",
                    Description = "Cá ngừ tươi nướng vừa chín tới, kèm khoai lang và rau củ. Nguồn protein và Omega-3 dồi dào.",
                    Calories = 380,
                    Protein = 42.0m,
                    Carbs = 30.0m,
                    Fat = 12.0m,
                    BasePrice = 105000,
                    IsActive = true,
                    CreatedAt = mealsSeedDate
                },
                new Meal
                {
                    Id = 10,
                    Name = "Bowl Quinoa Gà",
                    Ingredients = "[\"Quinoa\",\"Ức gà\",\"Bơ\",\"Cà chua\",\"Dưa chuột\",\"Hành tây đỏ\",\"Rau mầm\",\"Sốt tahini\",\"Chanh\"]",
                    Images = "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\",\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]",
                    Description = "Bowl quinoa đầy đủ dinh dưỡng với ức gà, rau củ tươi và sốt tahini. Món ăn healthy, giàu protein và chất xơ.",
                    Calories = 400,
                    Protein = 35.0m,
                    Carbs = 42.0m,
                    Fat = 12.5m,
                    BasePrice = 92000,
                    IsActive = true,
                    CreatedAt = mealsSeedDate
                }
            );

            // Seed Users (Admin and User)
            var adminId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var userId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            
            // Pre-hashed passwords: "Admin123!" and "User123!"
            // Generated using BCrypt.Net.BCrypt.HashPassword()
            // Admin123! hash
            var adminPasswordHash = "$2a$11$pxXoT3Q7rI/BJC7WrofzouutZ/Fa/zkmdCiv3yTMRC6Cx47v0uPXe";
            // User123! hash  
            var userPasswordHash = "$2a$11$5HBUykQQvHJfJ9fXFDIqxu6zsTjZcSVnf4SFXQAoSQl4h.UFmaTd2";

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

            // Seed UserNutritionProfile for user
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
                    Notes = "Tập gym 3-4 lần/tuần, muốn tăng cơ"
                }
            );

            // Seed UserAllergies
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
    }
}
