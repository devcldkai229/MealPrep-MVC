using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MealPrep.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliverySlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliverySlots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Ingredients = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Calories = table.Column<int>(type: "int", nullable: false),
                    Protein = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Carbs = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Fat = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmbeddingJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OtpCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OtpCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanMealTiers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    MealsPerDay = table.Column<int>(type: "int", nullable: false),
                    ExtraPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanMealTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanMealTiers_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NutritionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MealId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NutritionLogs_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NutritionLogs_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MealsPerDay = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDislikedMeals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MealId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDislikedMeals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDislikedMeals_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDislikedMeals_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNutritionProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeightCm = table.Column<int>(type: "int", nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Goal = table.Column<int>(type: "int", nullable: false),
                    ActivityLevel = table.Column<int>(type: "int", nullable: false),
                    DietPreference = table.Column<int>(type: "int", nullable: false),
                    MealsPerDay = table.Column<int>(type: "int", nullable: false),
                    CaloriesInDay = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNutritionProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNutritionProfiles_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyMenus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WeekStart = table.Column<DateOnly>(type: "date", nullable: false),
                    WeekEnd = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyMenus_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DeliverySlotId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryOrders_DeliverySlots_DeliverySlotId",
                        column: x => x.DeliverySlotId,
                        principalTable: "DeliverySlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DeliveryOrders_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DeliverySlotId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_DeliverySlots_DeliverySlotId",
                        column: x => x.DeliverySlotId,
                        principalTable: "DeliverySlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserAllergies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserNutritionProfileId = table.Column<int>(type: "int", nullable: false),
                    AllergyName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAllergies_UserNutritionProfiles_UserNutritionProfileId",
                        column: x => x.UserNutritionProfileId,
                        principalTable: "UserNutritionProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyMenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WeeklyMenuId = table.Column<int>(type: "int", nullable: false),
                    MealId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyMenuItems_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WeeklyMenuItems_WeeklyMenus_WeeklyMenuId",
                        column: x => x.WeeklyMenuId,
                        principalTable: "WeeklyMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryOrderId = table.Column<int>(type: "int", nullable: false),
                    MealId = table.Column<int>(type: "int", nullable: true),
                    MealNameSnapshot = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MealType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryOrderItems_DeliveryOrders_DeliveryOrderId",
                        column: x => x.DeliveryOrderId,
                        principalTable: "DeliveryOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryOrderItems_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    MealId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    Gateway = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OrderId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ResponseCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResponseMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RawResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Meals",
                columns: new[] { "Id", "BasePrice", "Calories", "Carbs", "CreatedAt", "Description", "EmbeddingJson", "Fat", "Images", "Ingredients", "IsActive", "Name", "Protein", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 85000m, 320, 18.2m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "?c gà nu?ng thom l?ng v?i s?t m?t ong d?m dà, kèm rau c? tuoi ngon. Món an giàu protein, ít calo, phù h?p cho ch? d? an kiêng và t?p luy?n.", null, 8.8m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"?c gà\",\"M?t ong\",\"T?i\",\"G?ng\",\"Hành tây\",\"D?u oliu\",\"Mu?i\",\"Tiêu\"]", true, "?c Gà Nu?ng M?t Ong", 45.5m, null },
                    { 2, 120000m, 480, 25.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá h?i tuoi ngon áp ch?o v?i l?p da giòn tan, kèm rau c? h?p. Ngu?n Omega-3 d?i dào, t?t cho tim m?ch và não b?.", null, 22.5m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá h?i\",\"Bo\",\"Chanh\",\"Thì là\",\"Khoai tây\",\"Bông c?i xanh\",\"Mu?i\",\"Tiêu\"]", true, "Cá H?i Áp Ch?o", 42.0m, null },
                    { 3, 95000m, 420, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t bò m?m xào v?i rau c? tuoi, s?t d?m dà. Món an giàu s?t và protein, phù h?p cho ngu?i t?p gym.", null, 18.5m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"Cà r?t\",\"?t chuông\",\"Hành tây\",\"N?m\",\"T?i\",\"G?ng\",\"Nu?c tuong\",\"D?u mè\"]", true, "Bò Xào Rau C?", 38.0m, null },
                    { 4, 90000m, 380, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "?c gà s?t teriyaki d?m dà ki?u Nh?t, kèm rau c? xào. Món an giàu protein, ít béo, phù h?p cho ngu?i t?p luy?n.", null, 10.5m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"?c gà\",\"Nu?c tuong\",\"M?t ong\",\"G?ng\",\"T?i\",\"Hành tây\",\"?t chuông\",\"D?u mè\",\"H?t mè\"]", true, "?c Gà S?t Teriyaki", 42.0m, null },
                    { 5, 105000m, 400, 30.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá ng? tuoi nu?ng v?a chín t?i, kèm khoai lang và rau c?. Ngu?n protein và Omega-3 d?i dào.", null, 12.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá ng?\",\"Chanh\",\"T?i\",\"Thì là\",\"Khoai lang\",\"Bông c?i xanh\",\"D?u oliu\",\"Mu?i\",\"Tiêu\"]", true, "Cá Ng? Nu?ng", 48.0m, null },
                    { 6, 88000m, 450, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà tây nu?ng thom l?ng v?i com g?o l?t và d?u xanh. Món an cân b?ng dinh du?ng, giàu protein và ch?t xo.", null, 10.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà tây\",\"G?o l?t\",\"Ð?u xanh\",\"Cà r?t\",\"Hành tây\",\"T?i\",\"Gia v? nu?ng\",\"D?u oliu\"]", true, "Gà Tây Nu?ng", 44.0m, null },
                    { 7, 150000m, 520, 35.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t bò bít t?t m?m ngon, kèm khoai tây nghi?n và rau c?. Ngu?n protein và s?t cao.", null, 22.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"Khoai tây nghi?n\",\"Bông c?i xanh\",\"Cà r?t\",\"T?i\",\"Bo\",\"Mu?i\",\"Tiêu\",\"Huong th?o\"]", true, "Th?t Bò Bít T?t", 46.0m, null },
                    { 8, 82000m, 350, 32.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "?c gà nu?ng v?i th?o m?c thom l?ng, kèm khoai lang và d?u xanh. Protein cao, ít béo.", null, 7.5m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"?c gà\",\"Huong th?o\",\"Thì là\",\"T?i\",\"Chanh\",\"D?u oliu\",\"Khoai lang\",\"Ð?u xanh\",\"Mu?i\"]", true, "?c Gà Nu?ng Th?o M?c", 43.0m, null },
                    { 9, 98000m, 380, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá thu nu?ng thom ngon, giàu Omega-3 và protein. Kèm khoai tây và rau c?.", null, 14.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá thu\",\"Chanh\",\"?t\",\"T?i\",\"G?ng\",\"Rau thom\",\"Khoai tây\",\"Cà chua\",\"D?u oliu\"]", true, "Cá Thu Nu?ng", 40.0m, null },
                    { 10, 92000m, 480, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t heo nu?ng s?t BBQ d?m dà, kèm rau c? nu?ng. Protein cao, huong v? d?m dà.", null, 19.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t heo\",\"S?t BBQ\",\"Hành tây\",\"?t chuông\",\"B?p\",\"Khoai tây\",\"T?i\",\"Gia v? nu?ng\"]", true, "Th?t Heo Nu?ng BBQ", 41.0m, null },
                    { 11, 88000m, 420, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nu?ng mu?i ?t cay n?ng, kèm com g?o l?t và rau c?. Protein cao, huong v? d?m dà.", null, 12.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Mu?i ?t\",\"T?i\",\"Chanh\",\"Rau thom\",\"G?o l?t\",\"Rau c?\",\"D?u an\"]", true, "Gà Nu?ng Mu?i ?t", 39.0m, null },
                    { 12, 75000m, 450, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá basa chiên giòn vàng, kèm rau s?ng và s?t tartar. Protein cao, giòn ngon.", null, 15.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá basa\",\"B?t chiên\",\"Tr?ng\",\"Bánh mì\",\"Rau s?ng\",\"S?t tartar\",\"Chanh\",\"D?u an\"]", true, "Cá Basa Chiên Giòn", 36.0m, null },
                    { 13, 110000m, 480, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bò kho d?m dà, th?t bò m?m ngon v?i cà r?t và hành tây. Protein và s?t cao.", null, 16.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"Cà r?t\",\"Hành tây\",\"G?ng\",\"S?\",\"Nu?c d?a\",\"Gia v?\",\"Bánh mì\",\"Rau thom\"]", true, "Bò Kho", 44.0m, null },
                    { 14, 95000m, 440, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nu?ng lá chanh thom l?ng, kèm com và rau c?. Protein cao, huong v? d?c trung.", null, 14.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Lá chanh\",\"S?\",\"T?i\",\"?t\",\"G?ng\",\"Nu?c m?m\",\"G?o\",\"Rau c?\"]", true, "Gà Nu?ng Lá Chanh", 41.0m, null },
                    { 15, 85000m, 360, 32.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá chép h?p xì d?u thom ngon, kèm com và rau c?. Protein cao, ít béo.", null, 10.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá chép\",\"Xì d?u\",\"G?ng\",\"Hành lá\",\"?t\",\"D?u mè\",\"G?o\",\"Rau c?\"]", true, "Cá Chép H?p Xì D?u", 38.0m, null },
                    { 16, 98000m, 420, 35.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t bò xào lan d?m dà, kèm com. Protein cao, huong v? d?m dà.", null, 16.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"Hành tây\",\"?t chuông\",\"Cà chua\",\"T?i\",\"G?ng\",\"Nu?c tuong\",\"D?u mè\",\"G?o\"]", true, "Th?t Bò Xào Lan", 40.0m, null },
                    { 17, 90000m, 400, 36.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà rang mu?i giòn ngon, kèm com và rau c?. Protein cao, don gi?n nhung ngon.", null, 13.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Mu?i\",\"T?i\",\"?t\",\"Hành lá\",\"G?o\",\"Rau c?\",\"D?u an\"]", true, "Gà Rang Mu?i", 38.0m, null },
                    { 18, 130000m, 450, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl cá h?i sashimi tuoi ngon, kèm g?o sushi và rau c?. Protein và Omega-3 cao.", null, 15.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá h?i\",\"G?o sushi\",\"Rong bi?n\",\"Dua chu?t\",\"Cà r?t\",\"S?t teriyaki\",\"Wasabi\",\"G?ng ngâm\"]", true, "Cá H?i Sashimi Bowl", 44.0m, null },
                    { 19, 105000m, 380, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t bò nu?ng l?i thom l?ng, kèm bánh tráng và rau s?ng. Protein cao, huong v? d?m dà.", null, 16.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"S?\",\"T?i\",\"?t\",\"Gia v? nu?ng\",\"Bánh tráng\",\"Rau s?ng\",\"Nu?c ch?m\"]", true, "Th?t Bò Nu?ng L?i", 39.0m, null },
                    { 20, 92000m, 410, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nu?ng m?t ong t?i ng?t ngào, kèm khoai tây và rau c?. Protein cao, huong v? d?c bi?t.", null, 14.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"M?t ong\",\"T?i\",\"G?ng\",\"Hành tây\",\"Khoai tây\",\"Rau c?\",\"D?u oliu\"]", true, "Gà Nu?ng M?t Ong T?i", 40.0m, null },
                    { 21, 75000m, 280, 15.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tuoi ngon v?i ?c gà nu?ng, rau xanh giòn, phô mai feta và h?t óc chó. Món an nh?, giàu ch?t xo và vitamin.", null, 14.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"?c gà\",\"Xà lách\",\"Cà chua bi\",\"Dua chu?t\",\"?t chuông\",\"D?u gi?m\",\"Phô mai feta\",\"H?t óc chó\"]", true, "Salad Gà Nu?ng", 28.0m, null },
                    { 22, 110000m, 250, 20.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tôm tuoi s?t cam chua ng?t, kèm rau c?. Món an giàu protein, ít calo, phù h?p cho ch? d? an kiêng.", null, 8.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm tuoi\",\"Cam\",\"M?t ong\",\"G?ng\",\"T?i\",\"?t\",\"Hành lá\",\"D?u oliu\",\"Mu?i\"]", true, "Tôm S?t Cam", 30.0m, null },
                    { 23, 95000m, 290, 18.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad cá ng? tuoi ngon, kèm rau xanh và qu? bo. Protein cao, ít calo, giàu Omega-3.", null, 12.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Cá ng?\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"Hành tây d?\",\"D?u gi?m\",\"Qu? bo\",\"H?t hu?ng duong\"]", true, "Salad Cá Ng?", 32.0m, null },
                    { 24, 78000m, 320, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà lu?c m?m ngon, kèm rau c? h?p. Ít calo, giàu protein, phù h?p cho ch? d? gi?m cân.", null, 9.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Cà r?t\",\"Bông c?i xanh\",\"Ð?u que\",\"Khoai tây\",\"Hành tây\",\"Gia v?\",\"Nu?c dùng\"]", true, "Gà Lu?c Rau C?", 35.0m, null },
                    { 25, 88000m, 300, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá h?p g?ng thom ngon, kèm com g?o l?t và rau c?. Ít calo, giàu protein.", null, 8.5m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"G?ng\",\"Hành lá\",\"?t\",\"Nu?c tuong\",\"D?u mè\",\"Rau c?\",\"G?o l?t\"]", true, "Cá H?p G?ng", 34.0m, null },
                    { 26, 82000m, 270, 16.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad qu? bo và gà tuoi ngon, giàu ch?t béo t?t và protein. Ít calo, b? du?ng.", null, 13.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"?c gà\",\"Qu? bo\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"Hành tây\",\"D?u oliu\",\"Chanh\",\"Mu?i\"]", true, "Salad Qu? Bo Gà", 26.0m, null },
                    { 27, 105000m, 240, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tôm h?p bia thom ngon, kèm com g?o l?t và rau c?. Ít calo, giàu protein.", null, 6.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm\",\"Bia\",\"S?\",\"?t\",\"Chanh\",\"Mu?i\",\"Rau c?\",\"G?o l?t\"]", true, "Tôm H?p Bia", 28.0m, null },
                    { 28, 75000m, 260, 20.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nu?ng không da ít béo, kèm rau c? và khoai lang. Protein cao, calo th?p.", null, 5.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"?c gà không da\",\"Gia v?\",\"Chanh\",\"Rau c? h?p\",\"Khoai lang\",\"D?u oliu\"]", true, "Gà Nu?ng Không Da", 38.0m, null },
                    { 29, 90000m, 280, 26.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá nu?ng gi?y b?c gi? nguyên huong v?, kèm com g?o l?t. Ít calo, giàu protein.", null, 7.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Chanh\",\"G?ng\",\"Hành lá\",\"?t\",\"Rau c?\",\"G?o l?t\",\"D?u oliu\"]", true, "Cá Nu?ng Gi?y B?c", 32.0m, null },
                    { 30, 65000m, 220, 12.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tr?ng lu?c tuoi ngon, giàu protein. Ít calo, b? du?ng.", null, 11.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Tr?ng\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"Hành tây\",\"D?u gi?m\",\"H?t chia\",\"Rau thom\"]", true, "Salad Tr?ng Lu?c", 18.0m, null },
                    { 31, 80000m, 290, 24.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà xào rau c? tuoi ngon, ít calo. Protein cao, giàu ch?t xo.", null, 8.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"?c gà\",\"Cà r?t\",\"?t chuông\",\"Bông c?i xanh\",\"N?m\",\"T?i\",\"D?u oliu\",\"Gia v?\"]", true, "Gà Xào Rau C?", 32.0m, null },
                    { 32, 115000m, 310, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá h?i nu?ng kèm rau c?, giàu Omega-3. Ít calo, b? du?ng.", null, 10.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá h?i\",\"Bông c?i xanh\",\"Cà r?t\",\"Khoai lang\",\"D?u oliu\",\"Chanh\",\"Gia v?\"]", true, "Cá H?i Nu?ng Rau C?", 36.0m, null },
                    { 33, 98000m, 260, 14.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tôm và bo tuoi ngon, giàu protein và ch?t béo t?t. Ít calo.", null, 12.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Tôm\",\"Qu? bo\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"Hành tây\",\"D?u gi?m\",\"Chanh\"]", true, "Salad Tôm Bo", 24.0m, null },
                    { 34, 78000m, 300, 26.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà lu?c m?m ngon ch?m mu?i tiêu, kèm rau c?. Ít calo, giàu protein.", null, 8.5m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Mu?i\",\"Tiêu\",\"Chanh\",\"Rau c? h?p\",\"G?o l?t\"]", true, "Gà Lu?c Ch?m Mu?i Tiêu", 34.0m, null },
                    { 35, 85000m, 280, 30.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá kho t? d?m dà, kèm com và rau c?. Ít calo, giàu protein.", null, 6.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Nu?c m?m\",\"Ðu?ng\",\"?t\",\"G?ng\",\"Hành tây\",\"G?o\",\"Rau c?\"]", true, "Cá Kho T?", 30.0m, null },
                    { 36, 72000m, 250, 16.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad ?c gà nu?ng tuoi ngon, giàu protein. Ít calo, b? du?ng.", null, 9.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"?c gà\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"?t chuông\",\"D?u gi?m\",\"H?t hu?ng duong\"]", true, "Salad ?c Gà Nu?ng", 30.0m, null },
                    { 37, 102000m, 270, 32.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tôm rang me chua ng?t, kèm com. Ít calo, giàu protein.", null, 7.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm\",\"Me\",\"Ðu?ng\",\"T?i\",\"?t\",\"Hành lá\",\"G?o\",\"Rau c?\"]", true, "Tôm Rang Me", 26.0m, null },
                    { 38, 76000m, 290, 24.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà h?p mu?i m?m ngon, kèm rau c?. Ít calo, giàu protein.", null, 8.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Mu?i\",\"G?ng\",\"Hành lá\",\"Rau c?\",\"G?o l?t\"]", true, "Gà H?p Mu?i", 33.0m, null },
                    { 39, 88000m, 260, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá nu?ng mu?i ?t cay n?ng, kèm rau c?. Ít calo, giàu protein.", null, 7.5m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Mu?i ?t\",\"Chanh\",\"Rau c?\",\"G?o l?t\",\"D?u oliu\"]", true, "Cá Nu?ng Mu?i ?t", 28.0m, null },
                    { 40, 68000m, 240, 14.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad cá ng? dóng h?p ti?n l?i, giàu protein. Ít calo, b? du?ng.", null, 10.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Cá ng?\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"Hành tây\",\"D?u gi?m\",\"Tr?ng lu?c\"]", true, "Salad Cá Ng? Ðóng H?p", 22.0m, null },
                    { 41, 88000m, 420, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà tây nu?ng thom l?ng v?i com g?o l?t và d?u xanh. Món an cân b?ng dinh du?ng, giàu protein và ch?t xo.", null, 10.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà tây\",\"G?o l?t\",\"Ð?u xanh\",\"Cà r?t\",\"Hành tây\",\"T?i\",\"Gia v? nu?ng\",\"D?u oliu\"]", true, "Com Gà Tây Nu?ng", 40.0m, null },
                    { 42, 92000m, 400, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl quinoa d?y d? dinh du?ng v?i ?c gà, rau c? tuoi và s?t tahini. Món an healthy, giàu protein và ch?t xo.", null, 12.5m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Quinoa\",\"?c gà\",\"Bo\",\"Cà chua\",\"Dua chu?t\",\"Hành tây d?\",\"Rau m?m\",\"S?t tahini\",\"Chanh\"]", true, "Bowl Quinoa Gà", 35.0m, null },
                    { 43, 85000m, 480, 50.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t kho tàu d?m dà, kèm tr?ng và com. Món an cân b?ng dinh du?ng, huong v? d?m dà.", null, 18.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t ba ch?\",\"Tr?ng\",\"Nu?c d?a\",\"Nu?c m?m\",\"Ðu?ng\",\"Hành tây\",\"G?o\",\"Dua chua\"]", true, "Com Th?t Kho Tàu", 32.0m, null },
                    { 44, 98000m, 520, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Su?n heo nu?ng m?t ong thom ngon, kèm com và rau c?. Cân b?ng dinh du?ng.", null, 22.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Su?n heo\",\"M?t ong\",\"T?i\",\"G?ng\",\"G?o\",\"Rau c?\",\"D?u oliu\"]", true, "Com Su?n Nu?ng", 35.0m, null },
                    { 45, 90000m, 450, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nu?ng mu?i ?t cay n?ng, kèm com và rau c?. Cân b?ng dinh du?ng, huong v? d?m dà.", null, 16.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Mu?i ?t\",\"T?i\",\"Chanh\",\"G?o\",\"Rau c?\",\"D?u an\"]", true, "Com Gà Nu?ng Mu?i ?t", 38.0m, null },
                    { 46, 88000m, 380, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá kho t? d?m dà, kèm com và rau c?. Cân b?ng dinh du?ng, huong v? truy?n th?ng.", null, 12.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Nu?c m?m\",\"Ðu?ng\",\"?t\",\"G?ng\",\"Hành tây\",\"G?o\",\"Rau c?\"]", true, "Com Cá Kho T?", 32.0m, null },
                    { 47, 102000m, 440, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t bò xào d?m dà, kèm com. Cân b?ng dinh du?ng, giàu protein.", null, 18.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"Hành tây\",\"?t chuông\",\"Cà chua\",\"T?i\",\"G?ng\",\"Nu?c tuong\",\"G?o\",\"D?u mè\"]", true, "Com Th?t Bò Xào", 36.0m, null },
                    { 48, 80000m, 400, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà lu?c m?m ngon, kèm com và rau c?. Cân b?ng dinh du?ng, don gi?n nhung ngon.", null, 12.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Mu?i\",\"G?ng\",\"Hành lá\",\"G?o\",\"Rau c?\",\"Nu?c dùng\"]", true, "Com Gà Lu?c", 35.0m, null },
                    { 49, 110000m, 420, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tôm rang me chua ng?t, kèm com. Cân b?ng dinh du?ng, huong v? d?c bi?t.", null, 14.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm\",\"Me\",\"Ðu?ng\",\"T?i\",\"?t\",\"Hành lá\",\"G?o\",\"Rau c?\"]", true, "Com Tôm Rang Me", 28.0m, null },
                    { 50, 85000m, 460, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá chiên giòn vàng, kèm com và rau s?ng. Cân b?ng dinh du?ng, giòn ngon.", null, 18.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"B?t chiên\",\"Tr?ng\",\"G?o\",\"Rau s?ng\",\"Nu?c m?m\",\"Chanh\",\"D?u an\"]", true, "Com Cá Chiên", 30.0m, null },
                    { 51, 95000m, 500, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t heo quay giòn tan, kèm com và dua chua. Cân b?ng dinh du?ng, huong v? d?m dà.", null, 22.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t heo quay\",\"G?o\",\"Dua chua\",\"Rau c?\",\"Nu?c m?m\",\"T?i\",\"?t\"]", true, "Com Th?t Heo Quay", 34.0m, null },
                    { 52, 92000m, 480, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà x?i m? thom l?ng, kèm com và rau c?. Cân b?ng dinh du?ng, huong v? d?c trung.", null, 20.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"M?\",\"T?i\",\"G?ng\",\"G?o\",\"Rau c?\",\"Nu?c m?m\"]", true, "Com Gà X?i M?", 36.0m, null },
                    { 53, 90000m, 390, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá lóc kho t? d?m dà, kèm com. Cân b?ng dinh du?ng, huong v? truy?n th?ng.", null, 13.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá lóc\",\"Nu?c m?m\",\"Ðu?ng\",\"?t\",\"G?ng\",\"Hành tây\",\"G?o\",\"Rau c?\"]", true, "Com Cá Lóc Kho T?", 34.0m, null },
                    { 54, 115000m, 460, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t bò nu?ng thom l?ng, kèm com và rau c?. Cân b?ng dinh du?ng, giàu protein.", null, 18.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"Gia v? nu?ng\",\"T?i\",\"G?ng\",\"G?o\",\"Rau c?\",\"D?u oliu\"]", true, "Com Th?t Bò Nu?ng", 40.0m, null },
                    { 55, 88000m, 520, 50.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà rán giòn vàng, kèm com và rau c?. Cân b?ng dinh du?ng, giòn ngon.", null, 22.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"B?t chiên\",\"Gia v?\",\"G?o\",\"Rau c?\",\"D?u an\"]", true, "Com Gà Rán", 32.0m, null },
                    { 56, 78000m, 470, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá basa chiên giòn, kèm com và rau s?ng. Cân b?ng dinh du?ng, giòn ngon.", null, 20.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá basa\",\"B?t chiên\",\"Tr?ng\",\"G?o\",\"Rau s?ng\",\"Nu?c m?m\",\"D?u an\"]", true, "Com Cá Basa Chiên", 28.0m, null },
                    { 57, 90000m, 490, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t heo nu?ng thom l?ng, kèm com và rau c?. Cân b?ng dinh du?ng, huong v? d?m dà.", null, 21.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t heo\",\"Gia v? nu?ng\",\"M?t ong\",\"T?i\",\"G?o\",\"Rau c?\",\"D?u oliu\"]", true, "Com Th?t Heo Nu?ng", 33.0m, null },
                    { 58, 85000m, 410, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà n?u n?m thom ngon, kèm com và rau c?. Cân b?ng dinh du?ng, b? du?ng.", null, 14.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"N?m\",\"Hành tây\",\"T?i\",\"G?ng\",\"G?o\",\"Rau c?\",\"Nu?c dùng\"]", true, "Com Gà N?u N?m", 34.0m, null },
                    { 59, 88000m, 370, 36.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá h?p xì d?u thom ngon, kèm com và rau c?. Cân b?ng dinh du?ng, ít béo.", null, 11.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Xì d?u\",\"G?ng\",\"Hành lá\",\"?t\",\"G?o\",\"Rau c?\",\"D?u mè\"]", true, "Com Cá H?p Xì D?u", 32.0m, null },
                    { 60, 110000m, 480, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t bò kho d?m dà, kèm com. Cân b?ng dinh du?ng, huong v? d?c trung.", null, 17.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"Cà r?t\",\"Hành tây\",\"G?ng\",\"S?\",\"Nu?c d?a\",\"G?o\",\"Rau thom\"]", true, "Com Th?t Bò Kho", 38.0m, null },
                    { 61, 75000m, 350, 55.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl quinoa v?i rau c? tuoi, s?t tahini. Món an vegan healthy, giàu protein th?c v?t và ch?t xo.", null, 10.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Quinoa\",\"Bông c?i xanh\",\"Cà r?t\",\"?t chuông\",\"Cà chua\",\"Dua chu?t\",\"Hành tây\",\"S?t tahini\",\"Chanh\"]", true, "Bowl Quinoa Rau C?", 12.0m, null },
                    { 62, 68000m, 280, 20.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad d?u hu nu?ng tuoi ngon, giàu protein th?c v?t. Món an vegan, ít calo.", null, 14.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ð?u hu\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"?t chuông\",\"Hành tây\",\"D?u gi?m\",\"H?t hu?ng duong\"]", true, "Salad Ð?u Hu Nu?ng", 18.0m, null },
                    { 63, 65000m, 380, 52.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ð?u hu chiên giòn, kèm com và rau c?. Món an vegan, giàu protein th?c v?t.", null, 12.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ð?u hu\",\"B?t chiên\",\"G?o\",\"Rau c?\",\"Nu?c tuong\",\"D?u an\",\"Hành lá\"]", true, "Com Ð?u Hu Chiên", 16.0m, null },
                    { 64, 70000m, 320, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl d?u lang v?i rau c?, giàu protein và ch?t xo. Món an vegan b? du?ng.", null, 8.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Ð?u lang\",\"Cà r?t\",\"C?n tây\",\"Hành tây\",\"Cà chua\",\"Gia v?\",\"Rau thom\",\"D?u oliu\"]", true, "Bowl Ð?u Lang Rau C?", 20.0m, null },
                    { 65, 72000m, 260, 18.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tempeh tuoi ngon, giàu protein th?c v?t. Món an vegan, ít calo.", null, 12.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Tempeh\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"Hành tây\",\"D?u gi?m\",\"H?t chia\",\"Rau thom\"]", true, "Salad Tempeh", 22.0m, null },
                    { 66, 68000m, 360, 50.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ð?u hu s?t cà chua d?m dà, kèm com. Món an vegan, giàu protein th?c v?t.", null, 10.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ð?u hu\",\"Cà chua\",\"Hành tây\",\"T?i\",\"G?ng\",\"G?o\",\"Rau c?\",\"D?u oliu\"]", true, "Com Ð?u Hu S?t Cà Chua", 15.0m, null },
                    { 67, 73000m, 340, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl d?u gà v?i rau c?, giàu protein và ch?t xo. Món an vegan b? du?ng.", null, 11.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Ð?u gà\",\"Bông c?i xanh\",\"Cà r?t\",\"?t chuông\",\"Hành tây\",\"S?t tahini\",\"Chanh\",\"D?u oliu\"]", true, "Bowl Ð?u Gà Rau C?", 18.0m, null },
                    { 68, 75000m, 300, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad qu? bo và d?u hu, giàu ch?t béo t?t và protein th?c v?t. Món an vegan.", null, 16.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ð?u hu\",\"Qu? bo\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"Hành tây\",\"D?u gi?m\",\"H?t hu?ng duong\"]", true, "Salad Qu? Bo Ð?u Hu", 14.0m, null },
                    { 69, 70000m, 370, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ð?u hu nu?ng thom l?ng, kèm com và rau c?. Món an vegan, giàu protein th?c v?t.", null, 13.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ð?u hu\",\"Gia v? nu?ng\",\"T?i\",\"G?ng\",\"G?o\",\"Rau c?\",\"D?u oliu\"]", true, "Com Ð?u Hu Nu?ng", 17.0m, null },
                    { 70, 78000m, 330, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl seitan v?i rau c?, giàu protein th?c v?t. Món an vegan, huong v? d?m dà.", null, 9.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Seitan\",\"Bông c?i xanh\",\"Cà r?t\",\"?t chuông\",\"Hành tây\",\"S?t teriyaki\",\"D?u oliu\"]", true, "Bowl Seitan Rau C?", 25.0m, null },
                    { 71, 60000m, 380, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tr?ng chiên thom ngon, kèm com và rau c?. Món an vegetarian, giàu protein.", null, 14.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Tr?ng\",\"Hành lá\",\"G?o\",\"Rau c?\",\"D?u an\",\"Nu?c m?m\"]", true, "Com Tr?ng Chiên", 20.0m, null },
                    { 72, 68000m, 350, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ð?u hu s?t n?m thom ngon, kèm com. Món an vegetarian, giàu protein th?c v?t.", null, 11.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ð?u hu\",\"N?m\",\"Hành tây\",\"T?i\",\"G?ng\",\"G?o\",\"Rau c?\",\"Nu?c tuong\"]", true, "Com Ð?u Hu S?t N?m", 16.0m, null },
                    { 73, 72000m, 320, 20.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tr?ng và qu? bo, giàu protein và ch?t béo t?t. Món an vegetarian.", null, 18.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Tr?ng\",\"Qu? bo\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"Hành tây\",\"D?u gi?m\",\"H?t chia\"]", true, "Salad Tr?ng Qu? Bo", 16.0m, null },
                    { 74, 75000m, 420, 50.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Com phô mai nu?ng thom l?ng, kèm rau c?. Món an vegetarian, giàu protein và canxi.", null, 16.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Phô mai\",\"G?o\",\"Rau c?\",\"Bo\",\"T?i\",\"Gia v?\"]", true, "Com Phô Mai Nu?ng", 18.0m, null },
                    { 75, 78000m, 360, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl tr?ng và quinoa v?i rau c?, giàu protein. Món an vegetarian b? du?ng.", null, 13.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Tr?ng\",\"Quinoa\",\"Bông c?i xanh\",\"Cà r?t\",\"?t chuông\",\"S?t tahini\",\"Chanh\"]", true, "Bowl Tr?ng Quinoa", 19.0m, null },
                    { 76, 70000m, 340, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ð?u hu xào rau c? tuoi ngon, kèm com. Món an vegetarian, giàu protein th?c v?t.", null, 10.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ð?u hu\",\"Cà r?t\",\"?t chuông\",\"Bông c?i xanh\",\"N?m\",\"T?i\",\"G?o\",\"Nu?c tuong\"]", true, "Com Ð?u Hu Xào Rau C?", 15.0m, null },
                    { 77, 72000m, 290, 18.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tr?ng và d?u hu, giàu protein. Món an vegetarian, ít calo.", null, 15.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Tr?ng\",\"Ð?u hu\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"D?u gi?m\",\"H?t hu?ng duong\"]", true, "Salad Tr?ng Ð?u Hu", 20.0m, null },
                    { 78, 62000m, 370, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tr?ng ?p la thom ngon, kèm com và rau c?. Món an vegetarian, giàu protein.", null, 15.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Tr?ng\",\"Hành lá\",\"G?o\",\"Rau c?\",\"D?u an\",\"Nu?c m?m\",\"Tiêu\"]", true, "Com Tr?ng ?p La", 19.0m, null },
                    { 79, 75000m, 350, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl d?u hu và quinoa v?i rau c?, giàu protein th?c v?t. Món an vegetarian.", null, 12.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Ð?u hu\",\"Quinoa\",\"Bông c?i xanh\",\"Cà r?t\",\"?t chuông\",\"S?t tahini\",\"Chanh\",\"D?u oliu\"]", true, "Bowl Ð?u Hu Quinoa", 17.0m, null },
                    { 80, 70000m, 390, 50.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ð?u hu chiên xù giòn tan, kèm com và rau c?. Món an vegetarian, giòn ngon.", null, 14.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ð?u hu\",\"B?t chiên xù\",\"G?o\",\"Rau c?\",\"Nu?c tuong\",\"D?u an\",\"Hành lá\"]", true, "Com Ð?u Hu Chiên Xù", 16.0m, null },
                    { 81, 85000m, 320, 8.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad gà keto, ít carb, giàu protein và ch?t béo t?t. Phù h?p cho ch? d? low-carb/keto.", null, 18.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"?c gà\",\"Xà lách\",\"Cà chua\",\"Dua chu?t\",\"Qu? bo\",\"D?u oliu\",\"Chanh\",\"H?t chia\"]", true, "Salad Gà Keto", 35.0m, null },
                    { 82, 120000m, 380, 10.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá h?i v?i rau c? keto, ít carb, giàu Omega-3. Phù h?p cho ch? d? low-carb/keto.", null, 24.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá h?i\",\"Bông c?i xanh\",\"Cà r?t\",\"Bo\",\"D?u oliu\",\"Chanh\",\"Gia v?\"]", true, "Cá H?i Rau C? Keto", 36.0m, null },
                    { 83, 98000m, 350, 12.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t bò xào rau c? keto, ít carb, giàu protein. Phù h?p cho ch? d? low-carb/keto.", null, 18.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"Bông c?i xanh\",\"?t chuông\",\"N?m\",\"T?i\",\"D?u oliu\",\"Gia v?\"]", true, "Th?t Bò Xào Rau C? Keto", 38.0m, null },
                    { 84, 88000m, 330, 9.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nu?ng v?i rau c? keto, ít carb, giàu protein. Phù h?p cho ch? d? low-carb/keto.", null, 16.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"?c gà\",\"Bông c?i xanh\",\"Cà r?t\",\"Bo\",\"D?u oliu\",\"Gia v? nu?ng\"]", true, "Gà Nu?ng Rau C? Keto", 40.0m, null },
                    { 85, 95000m, 300, 7.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad cá ng? keto, ít carb, giàu protein và Omega-3. Phù h?p cho ch? d? low-carb/keto.", null, 16.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Cá ng?\",\"Xà lách\",\"Qu? bo\",\"Dua chu?t\",\"D?u oliu\",\"Chanh\",\"H?t chia\"]", true, "Salad Cá Ng? Keto", 32.0m, null },
                    { 86, 105000m, 360, 11.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t bò nu?ng v?i rau c? keto, ít carb, giàu protein. Phù h?p cho ch? d? low-carb/keto.", null, 19.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"Bông c?i xanh\",\"Cà r?t\",\"Bo\",\"D?u oliu\",\"Gia v? nu?ng\"]", true, "Th?t Bò Nu?ng Rau C? Keto", 39.0m, null },
                    { 87, 90000m, 310, 8.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá h?p v?i rau c? keto, ít carb, giàu protein. Phù h?p cho ch? d? low-carb/keto.", null, 15.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Bông c?i xanh\",\"Cà r?t\",\"Bo\",\"D?u oliu\",\"G?ng\",\"Chanh\"]", true, "Cá H?p Rau C? Keto", 34.0m, null },
                    { 88, 82000m, 320, 10.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà xào rau c? keto, ít carb, giàu protein. Phù h?p cho ch? d? low-carb/keto.", null, 14.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"?c gà\",\"Bông c?i xanh\",\"?t chuông\",\"N?m\",\"T?i\",\"D?u oliu\",\"Gia v?\"]", true, "Gà Xào Rau C? Keto", 36.0m, null },
                    { 89, 110000m, 280, 6.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tôm keto, ít carb, giàu protein. Phù h?p cho ch? d? low-carb/keto.", null, 14.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm\",\"Xà lách\",\"Qu? bo\",\"Dua chu?t\",\"D?u oliu\",\"Chanh\",\"H?t chia\"]", true, "Salad Tôm Keto", 28.0m, null },
                    { 90, 100000m, 340, 9.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Th?t bò xào n?m keto, ít carb, giàu protein. Phù h?p cho ch? d? low-carb/keto.", null, 17.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"N?m\",\"Bông c?i xanh\",\"T?i\",\"D?u oliu\",\"Gia v?\"]", true, "Th?t Bò Xào N?m Keto", 37.0m, null },
                    { 91, 88000m, 420, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Com gà halal thom ngon, kèm rau c?. Món an halal, giàu protein.", null, 14.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà halal\",\"G?o\",\"Rau c?\",\"Gia v? halal\",\"D?u oliu\",\"Hành tây\"]", true, "Com Gà Halal", 38.0m, null },
                    { 92, 115000m, 460, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Com th?t bò halal d?m dà, kèm rau c?. Món an halal, giàu protein và s?t.", null, 18.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò halal\",\"G?o\",\"Rau c?\",\"Gia v? halal\",\"D?u oliu\",\"T?i\"]", true, "Com Th?t Bò Halal", 40.0m, null },
                    { 93, 92000m, 440, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nu?ng halal thom l?ng, kèm com và rau c?. Món an halal, giàu protein.", null, 16.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà halal\",\"Gia v? nu?ng halal\",\"G?o\",\"Rau c?\",\"D?u oliu\",\"Chanh\"]", true, "Com Gà Nu?ng Halal", 39.0m, null },
                    { 94, 125000m, 480, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Com th?t c?u halal d?m dà, kèm rau c?. Món an halal, giàu protein.", null, 22.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t c?u halal\",\"G?o\",\"Rau c?\",\"Gia v? halal\",\"D?u oliu\",\"Hành tây\"]", true, "Com Th?t C?u Halal", 42.0m, null },
                    { 95, 85000m, 400, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà xào halal thom ngon, kèm com. Món an halal, giàu protein.", null, 15.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà halal\",\"Hành tây\",\"?t chuông\",\"N?m\",\"G?o\",\"Gia v? halal\",\"D?u oliu\"]", true, "Com Gà Xào Halal", 36.0m, null },
                    { 96, 88000m, 410, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Com g?o l?t v?i gà nu?ng, không ch?a gluten. Món an gluten-free, giàu ch?t xo.", null, 13.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"G?o l?t\",\"Rau c?\",\"D?u oliu\",\"Gia v?\",\"Chanh\"]", true, "Com G?o L?t Gà Nu?ng", 37.0m, null },
                    { 97, 118000m, 390, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl quinoa v?i cá h?i, không ch?a gluten. Món an gluten-free, giàu Omega-3.", null, 16.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Cá h?i\",\"Quinoa\",\"Bông c?i xanh\",\"Cà r?t\",\"D?u oliu\",\"Chanh\",\"Gia v?\"]", true, "Bowl Quinoa Cá H?i", 34.0m, null },
                    { 98, 108000m, 450, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Com g?o l?t v?i th?t bò, không ch?a gluten. Món an gluten-free, giàu protein.", null, 17.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Th?t bò\",\"G?o l?t\",\"Rau c?\",\"D?u oliu\",\"Gia v?\",\"T?i\"]", true, "Com G?o L?t Th?t Bò", 38.0m, null },
                    { 99, 92000m, 400, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl quinoa v?i gà, không ch?a gluten. Món an gluten-free, giàu protein và ch?t xo.", null, 14.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Gà\",\"Quinoa\",\"Bông c?i xanh\",\"Cà r?t\",\"?t chuông\",\"D?u oliu\",\"Gia v?\"]", true, "Bowl Quinoa Gà", 36.0m, null },
                    { 100, 90000m, 380, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Com g?o l?t v?i cá nu?ng, không ch?a gluten. Món an gluten-free, giàu protein.", null, 12.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"G?o l?t\",\"Rau c?\",\"D?u oliu\",\"Chanh\",\"Gia v?\"]", true, "Com G?o L?t Cá Nu?ng", 32.0m, null }
                });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "BasePrice", "CreatedAt", "Description", "DurationDays", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, 300000m, new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Gói dang ký theo tu?n (7 ngày)", 7, true, "Weekly" },
                    { 2, 1000000m, new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Gói dang ký theo tháng (30 ngày)", 30, true, "Monthly" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "User" }
                });

            migrationBuilder.InsertData(
                table: "PlanMealTiers",
                columns: new[] { "Id", "CreatedAt", "ExtraPrice", "IsActive", "MealsPerDay", "PlanId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), 0m, true, 1, 1 },
                    { 2, new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), 150000m, true, 2, 1 },
                    { 3, new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), 280000m, true, 3, 1 },
                    { 4, new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), 0m, true, 1, 2 },
                    { 5, new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), 500000m, true, 2, 2 },
                    { 6, new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), 900000m, true, 3, 2 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Age", "AvatarUrl", "CreatedAtUtc", "Email", "FullName", "Gender", "IsActive", "LastLoginAtUtc", "PasswordHash", "PhoneNumber", "RoleId" },
                values: new object[,]
                {
                    { new Guid("33333333-3333-3333-3333-333333333333"), 30, "", new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), "admin@mealprep.com", "Administrator", 1, true, null, "$2a$11$pxXoT3Q7rI/BJC7WrofzouutZ/Fa/zkmdCiv3yTMRC6Cx47v0uPXe", "0901234567", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 28, "", new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), "user@mealprep.com", "Nguy?n Van A", 1, true, null, "$2a$11$5HBUykQQvHJfJ9fXFDIqxu6zsTjZcSVnf4SFXQAoSQl4h.UFmaTd2", "0907654321", new Guid("22222222-2222-2222-2222-222222222222") }
                });

            migrationBuilder.InsertData(
                table: "UserNutritionProfiles",
                columns: new[] { "Id", "ActivityLevel", "AppUserId", "CaloriesInDay", "DietPreference", "Goal", "HeightCm", "MealsPerDay", "Notes", "WeightKg" },
                values: new object[] { 1, 3, new Guid("44444444-4444-4444-4444-444444444444"), null, 0, 2, 175, 3, "T?p gym 3-4 l?n/tu?n, mu?n tang co, thích an ng?t nhi?u và cay ít", 75.5m });

            migrationBuilder.InsertData(
                table: "UserAllergies",
                columns: new[] { "Id", "AllergyName", "UserNutritionProfileId" },
                values: new object[,]
                {
                    { 1, "Ð?u ph?ng", 1 },
                    { 2, "H?i s?n", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrderItems_DeliveryOrderId",
                table: "DeliveryOrderItems",
                column: "DeliveryOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrderItems_MealId",
                table: "DeliveryOrderItems",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrders_DeliverySlotId",
                table: "DeliveryOrders",
                column: "DeliverySlotId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrders_SubscriptionId",
                table: "DeliveryOrders",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionLogs_AppUserId",
                table: "NutritionLogs",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_NutritionLogs_MealId",
                table: "NutritionLogs",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MealId",
                table: "OrderItems",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AppUserId",
                table: "Orders",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliverySlotId",
                table: "Orders",
                column: "DeliverySlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SubscriptionId",
                table: "Orders",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AppUserId",
                table: "Payments",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentCode",
                table: "Payments",
                column: "PaymentCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                table: "Payments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PaymentId",
                table: "PaymentTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanMealTiers_PlanId_MealsPerDay",
                table: "PlanMealTiers",
                columns: new[] { "PlanId", "MealsPerDay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Name",
                table: "Plans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_AppUserId",
                table: "Subscriptions",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAllergies_UserNutritionProfileId",
                table: "UserAllergies",
                column: "UserNutritionProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDislikedMeals_AppUserId_MealId",
                table: "UserDislikedMeals",
                columns: new[] { "AppUserId", "MealId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDislikedMeals_MealId",
                table: "UserDislikedMeals",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNutritionProfiles_AppUserId",
                table: "UserNutritionProfiles",
                column: "AppUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyMenuItems_MealId",
                table: "WeeklyMenuItems",
                column: "MealId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyMenuItems_WeeklyMenuId",
                table: "WeeklyMenuItems",
                column: "WeeklyMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyMenus_CreatedByUserId",
                table: "WeeklyMenus",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryOrderItems");

            migrationBuilder.DropTable(
                name: "NutritionLogs");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "OtpCodes");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "PlanMealTiers");

            migrationBuilder.DropTable(
                name: "UserAllergies");

            migrationBuilder.DropTable(
                name: "UserDislikedMeals");

            migrationBuilder.DropTable(
                name: "WeeklyMenuItems");

            migrationBuilder.DropTable(
                name: "DeliveryOrders");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "UserNutritionProfiles");

            migrationBuilder.DropTable(
                name: "Meals");

            migrationBuilder.DropTable(
                name: "WeeklyMenus");

            migrationBuilder.DropTable(
                name: "DeliverySlots");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
