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
        
        // New feedback entity
        public DbSet<MealRating> MealRatings => Set<MealRating>();

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

            // MealRating
            modelBuilder.Entity<MealRating>(entity =>
            {
                // Unique constraint: User chỉ rate 1 lần mỗi DeliveryOrderItem
                entity.HasIndex(r => new { r.AppUserId, r.DeliveryOrderItemId })
                    .IsUnique();

                entity.HasOne(r => r.AppUser)
                    .WithMany()
                    .HasForeignKey(r => r.AppUserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.DeliveryOrderItem)
                    .WithMany()
                    .HasForeignKey(r => r.DeliveryOrderItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Meal)
                    .WithMany()
                    .HasForeignKey(r => r.MealId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Use centralized seed data
            SeedData.SeedAll(modelBuilder);
        }
    }
}
