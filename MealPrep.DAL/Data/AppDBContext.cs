using MealPrep.DAL.Entities;
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
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<DeliverySlot> DeliverySlots => Set<DeliverySlot>();
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
                    .HasDefaultValueSql("NOW()");

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

                entity.Property(p => p.DailyBudget)
                    .HasPrecision(10, 2);
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
                entity.Property(m => m.Price).HasPrecision(10, 2);
            });

            // Order N-1 với AppUser
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(o => o.AppUser)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(o => o.AppUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Subscription N-1 với AppUser
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasOne(s => s.AppUser)
                    .WithMany(u => u.Subscriptions)
                    .HasForeignKey(s => s.AppUserId)
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
        }
    }
}
