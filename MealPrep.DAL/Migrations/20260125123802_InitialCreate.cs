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
                    { 1, 85000m, 320, 18.2m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ức gà nướng thơm lừng với sốt mật ong đậm đà, kèm rau củ tươi ngon. Món ăn giàu protein, ít calo, phù hợp cho chế độ ăn kiêng và tập luyện.", null, 8.8m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Ức gà\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Hành tây\",\"Dầu oliu\",\"Muối\",\"Tiêu\"]", true, "Ức Gà Nướng Mật Ong", 45.5m, null },
                    { 2, 120000m, 480, 25.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá hồi tươi ngon áp chảo với lớp da giòn tan, kèm rau củ hấp. Nguồn Omega-3 dồi dào, tốt cho tim mạch và não bộ.", null, 22.5m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá hồi\",\"Bơ\",\"Chanh\",\"Thì là\",\"Khoai tây\",\"Bông cải xanh\",\"Muối\",\"Tiêu\"]", true, "Cá Hồi Áp Chảo", 42.0m, null },
                    { 3, 95000m, 420, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò mềm xào với rau củ tươi, sốt đậm đà. Món ăn giàu sắt và protein, phù hợp cho người tập gym.", null, 18.5m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Nấm\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Dầu mè\"]", true, "Bò Xào Rau Củ", 38.0m, null },
                    { 4, 90000m, 380, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ức gà sốt teriyaki đậm đà kiểu Nhật, kèm rau củ xào. Món ăn giàu protein, ít béo, phù hợp cho người tập luyện.", null, 10.5m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Ức gà\",\"Nước tương\",\"Mật ong\",\"Gừng\",\"Tỏi\",\"Hành tây\",\"Ớt chuông\",\"Dầu mè\",\"Hạt mè\"]", true, "Ức Gà Sốt Teriyaki", 42.0m, null },
                    { 5, 105000m, 400, 30.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá ngừ tươi nướng vừa chín tới, kèm khoai lang và rau củ. Nguồn protein và Omega-3 dồi dào.", null, 12.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá ngừ\",\"Chanh\",\"Tỏi\",\"Thì là\",\"Khoai lang\",\"Bông cải xanh\",\"Dầu oliu\",\"Muối\",\"Tiêu\"]", true, "Cá Ngừ Nướng", 48.0m, null },
                    { 6, 88000m, 450, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà tây nướng thơm lừng với cơm gạo lứt và đậu xanh. Món ăn cân bằng dinh dưỡng, giàu protein và chất xơ.", null, 10.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà tây\",\"Gạo lứt\",\"Đậu xanh\",\"Cà rốt\",\"Hành tây\",\"Tỏi\",\"Gia vị nướng\",\"Dầu oliu\"]", true, "Gà Tây Nướng", 44.0m, null },
                    { 7, 150000m, 520, 35.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò bít tết mềm ngon, kèm khoai tây nghiền và rau củ. Nguồn protein và sắt cao.", null, 22.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Khoai tây nghiền\",\"Bông cải xanh\",\"Cà rốt\",\"Tỏi\",\"Bơ\",\"Muối\",\"Tiêu\",\"Hương thảo\"]", true, "Thịt Bò Bít Tết", 46.0m, null },
                    { 8, 82000m, 350, 32.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ức gà nướng với thảo mộc thơm lừng, kèm khoai lang và đậu xanh. Protein cao, ít béo.", null, 7.5m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Ức gà\",\"Hương thảo\",\"Thì là\",\"Tỏi\",\"Chanh\",\"Dầu oliu\",\"Khoai lang\",\"Đậu xanh\",\"Muối\"]", true, "Ức Gà Nướng Thảo Mộc", 43.0m, null },
                    { 9, 98000m, 380, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá thu nướng thơm ngon, giàu Omega-3 và protein. Kèm khoai tây và rau củ.", null, 14.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá thu\",\"Chanh\",\"Ớt\",\"Tỏi\",\"Gừng\",\"Rau thơm\",\"Khoai tây\",\"Cà chua\",\"Dầu oliu\"]", true, "Cá Thu Nướng", 40.0m, null },
                    { 10, 92000m, 480, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt heo nướng sốt BBQ đậm đà, kèm rau củ nướng. Protein cao, hương vị đậm đà.", null, 19.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt heo\",\"Sốt BBQ\",\"Hành tây\",\"Ớt chuông\",\"Bắp\",\"Khoai tây\",\"Tỏi\",\"Gia vị nướng\"]", true, "Thịt Heo Nướng BBQ", 41.0m, null },
                    { 11, 88000m, 420, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nướng muối ớt cay nồng, kèm cơm gạo lứt và rau củ. Protein cao, hương vị đậm đà.", null, 12.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Muối ớt\",\"Tỏi\",\"Chanh\",\"Rau thơm\",\"Gạo lứt\",\"Rau củ\",\"Dầu ăn\"]", true, "Gà Nướng Muối Ớt", 39.0m, null },
                    { 12, 75000m, 450, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá basa chiên giòn vàng, kèm rau sống và sốt tartar. Protein cao, giòn ngon.", null, 15.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá basa\",\"Bột chiên\",\"Trứng\",\"Bánh mì\",\"Rau sống\",\"Sốt tartar\",\"Chanh\",\"Dầu ăn\"]", true, "Cá Basa Chiên Giòn", 36.0m, null },
                    { 13, 110000m, 480, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bò kho đậm đà, thịt bò mềm ngon với cà rốt và hành tây. Protein và sắt cao.", null, 16.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Cà rốt\",\"Hành tây\",\"Gừng\",\"Sả\",\"Nước dừa\",\"Gia vị\",\"Bánh mì\",\"Rau thơm\"]", true, "Bò Kho", 44.0m, null },
                    { 14, 95000m, 440, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nướng lá chanh thơm lừng, kèm cơm và rau củ. Protein cao, hương vị đặc trưng.", null, 14.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Lá chanh\",\"Sả\",\"Tỏi\",\"Ớt\",\"Gừng\",\"Nước mắm\",\"Gạo\",\"Rau củ\"]", true, "Gà Nướng Lá Chanh", 41.0m, null },
                    { 15, 85000m, 360, 32.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá chép hấp xì dầu thơm ngon, kèm cơm và rau củ. Protein cao, ít béo.", null, 10.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá chép\",\"Xì dầu\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Dầu mè\",\"Gạo\",\"Rau củ\"]", true, "Cá Chép Hấp Xì Dầu", 38.0m, null },
                    { 16, 98000m, 420, 35.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò xào lăn đậm đà, kèm cơm. Protein cao, hương vị đậm đà.", null, 16.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Hành tây\",\"Ớt chuông\",\"Cà chua\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Dầu mè\",\"Gạo\"]", true, "Thịt Bò Xào Lăn", 40.0m, null },
                    { 17, 90000m, 400, 36.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà rang muối giòn ngon, kèm cơm và rau củ. Protein cao, đơn giản nhưng ngon.", null, 13.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Muối\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Dầu ăn\"]", true, "Gà Rang Muối", 38.0m, null },
                    { 18, 130000m, 450, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl cá hồi sashimi tươi ngon, kèm gạo sushi và rau củ. Protein và Omega-3 cao.", null, 15.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá hồi\",\"Gạo sushi\",\"Rong biển\",\"Dưa chuột\",\"Cà rốt\",\"Sốt teriyaki\",\"Wasabi\",\"Gừng ngâm\"]", true, "Cá Hồi Sashimi Bowl", 44.0m, null },
                    { 19, 105000m, 380, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò nướng lụi thơm lừng, kèm bánh tráng và rau sống. Protein cao, hương vị đậm đà.", null, 16.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Sả\",\"Tỏi\",\"Ớt\",\"Gia vị nướng\",\"Bánh tráng\",\"Rau sống\",\"Nước chấm\"]", true, "Thịt Bò Nướng Lụi", 39.0m, null },
                    { 20, 92000m, 410, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nướng mật ong tỏi ngọt ngào, kèm khoai tây và rau củ. Protein cao, hương vị đặc biệt.", null, 14.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Hành tây\",\"Khoai tây\",\"Rau củ\",\"Dầu oliu\"]", true, "Gà Nướng Mật Ong Tỏi", 40.0m, null },
                    { 21, 75000m, 280, 15.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tươi ngon với ức gà nướng, rau xanh giòn, phô mai feta và hạt óc chó. Món ăn nhẹ, giàu chất xơ và vitamin.", null, 14.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ức gà\",\"Xà lách\",\"Cà chua bi\",\"Dưa chuột\",\"Ớt chuông\",\"Dầu giấm\",\"Phô mai feta\",\"Hạt óc chó\"]", true, "Salad Gà Nướng", 28.0m, null },
                    { 22, 110000m, 250, 20.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tôm tươi sốt cam chua ngọt, kèm rau củ. Món ăn giàu protein, ít calo, phù hợp cho chế độ ăn kiêng.", null, 8.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm tươi\",\"Cam\",\"Mật ong\",\"Gừng\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Dầu oliu\",\"Muối\"]", true, "Tôm Sốt Cam", 30.0m, null },
                    { 23, 95000m, 290, 18.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad cá ngừ tươi ngon, kèm rau xanh và quả bơ. Protein cao, ít calo, giàu Omega-3.", null, 12.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Cá ngừ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây đỏ\",\"Dầu giấm\",\"Quả bơ\",\"Hạt hướng dương\"]", true, "Salad Cá Ngừ", 32.0m, null },
                    { 24, 78000m, 320, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà luộc mềm ngon, kèm rau củ hấp. Ít calo, giàu protein, phù hợp cho chế độ giảm cân.", null, 9.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Cà rốt\",\"Bông cải xanh\",\"Đậu que\",\"Khoai tây\",\"Hành tây\",\"Gia vị\",\"Nước dùng\"]", true, "Gà Luộc Rau Củ", 35.0m, null },
                    { 25, 88000m, 300, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá hấp gừng thơm ngon, kèm cơm gạo lứt và rau củ. Ít calo, giàu protein.", null, 8.5m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Nước tương\",\"Dầu mè\",\"Rau củ\",\"Gạo lứt\"]", true, "Cá Hấp Gừng", 34.0m, null },
                    { 26, 82000m, 270, 16.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad quả bơ và gà tươi ngon, giàu chất béo tốt và protein. Ít calo, bổ dưỡng.", null, 13.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ức gà\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu oliu\",\"Chanh\",\"Muối\"]", true, "Salad Quả Bơ Gà", 26.0m, null },
                    { 27, 105000m, 240, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tôm hấp bia thơm ngon, kèm cơm gạo lứt và rau củ. Ít calo, giàu protein.", null, 6.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm\",\"Bia\",\"Sả\",\"Ớt\",\"Chanh\",\"Muối\",\"Rau củ\",\"Gạo lứt\"]", true, "Tôm Hấp Bia", 28.0m, null },
                    { 28, 75000m, 260, 20.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nướng không da ít béo, kèm rau củ và khoai lang. Protein cao, calo thấp.", null, 5.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Ức gà không da\",\"Gia vị\",\"Chanh\",\"Rau củ hấp\",\"Khoai lang\",\"Dầu oliu\"]", true, "Gà Nướng Không Da", 38.0m, null },
                    { 29, 90000m, 280, 26.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá nướng giấy bạc giữ nguyên hương vị, kèm cơm gạo lứt. Ít calo, giàu protein.", null, 7.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Chanh\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Rau củ\",\"Gạo lứt\",\"Dầu oliu\"]", true, "Cá Nướng Giấy Bạc", 32.0m, null },
                    { 30, 65000m, 220, 12.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad trứng luộc tươi ngon, giàu protein. Ít calo, bổ dưỡng.", null, 11.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Trứng\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt chia\",\"Rau thơm\"]", true, "Salad Trứng Luộc", 18.0m, null },
                    { 31, 80000m, 290, 24.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà xào rau củ tươi ngon, ít calo. Protein cao, giàu chất xơ.", null, 8.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Ức gà\",\"Cà rốt\",\"Ớt chuông\",\"Bông cải xanh\",\"Nấm\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]", true, "Gà Xào Rau Củ", 32.0m, null },
                    { 32, 115000m, 310, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá hồi nướng kèm rau củ, giàu Omega-3. Ít calo, bổ dưỡng.", null, 10.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá hồi\",\"Bông cải xanh\",\"Cà rốt\",\"Khoai lang\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]", true, "Cá Hồi Nướng Rau Củ", 36.0m, null },
                    { 33, 98000m, 260, 14.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tôm và bơ tươi ngon, giàu protein và chất béo tốt. Ít calo.", null, 12.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Tôm\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Chanh\"]", true, "Salad Tôm Bơ", 24.0m, null },
                    { 34, 78000m, 300, 26.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà luộc mềm ngon chấm muối tiêu, kèm rau củ. Ít calo, giàu protein.", null, 8.5m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Muối\",\"Tiêu\",\"Chanh\",\"Rau củ hấp\",\"Gạo lứt\"]", true, "Gà Luộc Chấm Muối Tiêu", 34.0m, null },
                    { 35, 85000m, 280, 30.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá kho tộ đậm đà, kèm cơm và rau củ. Ít calo, giàu protein.", null, 6.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Nước mắm\",\"Đường\",\"Ớt\",\"Gừng\",\"Hành tây\",\"Gạo\",\"Rau củ\"]", true, "Cá Kho Tộ", 30.0m, null },
                    { 36, 72000m, 250, 16.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad ức gà nướng tươi ngon, giàu protein. Ít calo, bổ dưỡng.", null, 9.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ức gà\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Ớt chuông\",\"Dầu giấm\",\"Hạt hướng dương\"]", true, "Salad Ức Gà Nướng", 30.0m, null },
                    { 37, 102000m, 270, 32.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tôm rang me chua ngọt, kèm cơm. Ít calo, giàu protein.", null, 7.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm\",\"Me\",\"Đường\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Gạo\",\"Rau củ\"]", true, "Tôm Rang Me", 26.0m, null },
                    { 38, 76000m, 290, 24.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà hấp muối mềm ngon, kèm rau củ. Ít calo, giàu protein.", null, 8.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Muối\",\"Gừng\",\"Hành lá\",\"Rau củ\",\"Gạo lứt\"]", true, "Gà Hấp Muối", 33.0m, null },
                    { 39, 88000m, 260, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá nướng muối ớt cay nồng, kèm rau củ. Ít calo, giàu protein.", null, 7.5m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Muối ớt\",\"Chanh\",\"Rau củ\",\"Gạo lứt\",\"Dầu oliu\"]", true, "Cá Nướng Muối Ớt", 28.0m, null },
                    { 40, 68000m, 240, 14.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad cá ngừ đóng hộp tiện lợi, giàu protein. Ít calo, bổ dưỡng.", null, 10.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Cá ngừ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Trứng luộc\"]", true, "Salad Cá Ngừ Đóng Hộp", 22.0m, null },
                    { 41, 88000m, 420, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà tây nướng thơm lừng với cơm gạo lứt và đậu xanh. Món ăn cân bằng dinh dưỡng, giàu protein và chất xơ.", null, 10.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà tây\",\"Gạo lứt\",\"Đậu xanh\",\"Cà rốt\",\"Hành tây\",\"Tỏi\",\"Gia vị nướng\",\"Dầu oliu\"]", true, "Cơm Gà Tây Nướng", 40.0m, null },
                    { 42, 92000m, 400, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl quinoa đầy đủ dinh dưỡng với ức gà, rau củ tươi và sốt tahini. Món ăn healthy, giàu protein và chất xơ.", null, 12.5m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Quinoa\",\"Ức gà\",\"Bơ\",\"Cà chua\",\"Dưa chuột\",\"Hành tây đỏ\",\"Rau mầm\",\"Sốt tahini\",\"Chanh\"]", true, "Bowl Quinoa Gà", 35.0m, null },
                    { 43, 85000m, 480, 50.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt kho tàu đậm đà, kèm trứng và cơm. Món ăn cân bằng dinh dưỡng, hương vị đậm đà.", null, 18.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt ba chỉ\",\"Trứng\",\"Nước dừa\",\"Nước mắm\",\"Đường\",\"Hành tây\",\"Gạo\",\"Dưa chua\"]", true, "Cơm Thịt Kho Tàu", 32.0m, null },
                    { 44, 98000m, 520, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Sườn heo nướng mật ong thơm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng.", null, 22.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Sườn heo\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", true, "Cơm Sườn Nướng", 35.0m, null },
                    { 45, 90000m, 450, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nướng muối ớt cay nồng, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị đậm đà.", null, 16.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Muối ớt\",\"Tỏi\",\"Chanh\",\"Gạo\",\"Rau củ\",\"Dầu ăn\"]", true, "Cơm Gà Nướng Muối Ớt", 38.0m, null },
                    { 46, 88000m, 380, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá kho tộ đậm đà, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị truyền thống.", null, 12.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Nước mắm\",\"Đường\",\"Ớt\",\"Gừng\",\"Hành tây\",\"Gạo\",\"Rau củ\"]", true, "Cơm Cá Kho Tộ", 32.0m, null },
                    { 47, 102000m, 440, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò xào đậm đà, kèm cơm. Cân bằng dinh dưỡng, giàu protein.", null, 18.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Hành tây\",\"Ớt chuông\",\"Cà chua\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Gạo\",\"Dầu mè\"]", true, "Cơm Thịt Bò Xào", 36.0m, null },
                    { 48, 80000m, 400, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà luộc mềm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng, đơn giản nhưng ngon.", null, 12.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Muối\",\"Gừng\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Nước dùng\"]", true, "Cơm Gà Luộc", 35.0m, null },
                    { 49, 110000m, 420, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tôm rang me chua ngọt, kèm cơm. Cân bằng dinh dưỡng, hương vị đặc biệt.", null, 14.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm\",\"Me\",\"Đường\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Gạo\",\"Rau củ\"]", true, "Cơm Tôm Rang Me", 28.0m, null },
                    { 50, 85000m, 460, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá chiên giòn vàng, kèm cơm và rau sống. Cân bằng dinh dưỡng, giòn ngon.", null, 18.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Bột chiên\",\"Trứng\",\"Gạo\",\"Rau sống\",\"Nước mắm\",\"Chanh\",\"Dầu ăn\"]", true, "Cơm Cá Chiên", 30.0m, null },
                    { 51, 95000m, 500, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt heo quay giòn tan, kèm cơm và dưa chua. Cân bằng dinh dưỡng, hương vị đậm đà.", null, 22.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt heo quay\",\"Gạo\",\"Dưa chua\",\"Rau củ\",\"Nước mắm\",\"Tỏi\",\"Ớt\"]", true, "Cơm Thịt Heo Quay", 34.0m, null },
                    { 52, 92000m, 480, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà xối mỡ thơm lừng, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị đặc trưng.", null, 20.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Mỡ\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Nước mắm\"]", true, "Cơm Gà Xối Mỡ", 36.0m, null },
                    { 53, 90000m, 390, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá lóc kho tộ đậm đà, kèm cơm. Cân bằng dinh dưỡng, hương vị truyền thống.", null, 13.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá lóc\",\"Nước mắm\",\"Đường\",\"Ớt\",\"Gừng\",\"Hành tây\",\"Gạo\",\"Rau củ\"]", true, "Cơm Cá Lóc Kho Tộ", 34.0m, null },
                    { 54, 115000m, 460, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò nướng thơm lừng, kèm cơm và rau củ. Cân bằng dinh dưỡng, giàu protein.", null, 18.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Gia vị nướng\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", true, "Cơm Thịt Bò Nướng", 40.0m, null },
                    { 55, 88000m, 520, 50.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà rán giòn vàng, kèm cơm và rau củ. Cân bằng dinh dưỡng, giòn ngon.", null, 22.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Bột chiên\",\"Gia vị\",\"Gạo\",\"Rau củ\",\"Dầu ăn\"]", true, "Cơm Gà Rán", 32.0m, null },
                    { 56, 78000m, 470, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá basa chiên giòn, kèm cơm và rau sống. Cân bằng dinh dưỡng, giòn ngon.", null, 20.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá basa\",\"Bột chiên\",\"Trứng\",\"Gạo\",\"Rau sống\",\"Nước mắm\",\"Dầu ăn\"]", true, "Cơm Cá Basa Chiên", 28.0m, null },
                    { 57, 90000m, 490, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt heo nướng thơm lừng, kèm cơm và rau củ. Cân bằng dinh dưỡng, hương vị đậm đà.", null, 21.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt heo\",\"Gia vị nướng\",\"Mật ong\",\"Tỏi\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", true, "Cơm Thịt Heo Nướng", 33.0m, null },
                    { 58, 85000m, 410, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nấu nấm thơm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng, bổ dưỡng.", null, 14.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Nấm\",\"Hành tây\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Nước dùng\"]", true, "Cơm Gà Nấu Nấm", 34.0m, null },
                    { 59, 88000m, 370, 36.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá hấp xì dầu thơm ngon, kèm cơm và rau củ. Cân bằng dinh dưỡng, ít béo.", null, 11.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Xì dầu\",\"Gừng\",\"Hành lá\",\"Ớt\",\"Gạo\",\"Rau củ\",\"Dầu mè\"]", true, "Cơm Cá Hấp Xì Dầu", 32.0m, null },
                    { 60, 110000m, 480, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò kho đậm đà, kèm cơm. Cân bằng dinh dưỡng, hương vị đặc trưng.", null, 17.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Cà rốt\",\"Hành tây\",\"Gừng\",\"Sả\",\"Nước dừa\",\"Gạo\",\"Rau thơm\"]", true, "Cơm Thịt Bò Kho", 38.0m, null },
                    { 61, 75000m, 350, 55.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl quinoa với rau củ tươi, sốt tahini. Món ăn vegan healthy, giàu protein thực vật và chất xơ.", null, 10.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Sốt tahini\",\"Chanh\"]", true, "Bowl Quinoa Rau Củ", 12.0m, null },
                    { 62, 68000m, 280, 20.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad đậu hũ nướng tươi ngon, giàu protein thực vật. Món ăn vegan, ít calo.", null, 14.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Đậu hũ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Ớt chuông\",\"Hành tây\",\"Dầu giấm\",\"Hạt hướng dương\"]", true, "Salad Đậu Hũ Nướng", 18.0m, null },
                    { 63, 65000m, 380, 52.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Đậu hũ chiên giòn, kèm cơm và rau củ. Món ăn vegan, giàu protein thực vật.", null, 12.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Đậu hũ\",\"Bột chiên\",\"Gạo\",\"Rau củ\",\"Nước tương\",\"Dầu ăn\",\"Hành lá\"]", true, "Cơm Đậu Hũ Chiên", 16.0m, null },
                    { 64, 70000m, 320, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl đậu lăng với rau củ, giàu protein và chất xơ. Món ăn vegan bổ dưỡng.", null, 8.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Đậu lăng\",\"Cà rốt\",\"Cần tây\",\"Hành tây\",\"Cà chua\",\"Gia vị\",\"Rau thơm\",\"Dầu oliu\"]", true, "Bowl Đậu Lăng Rau Củ", 20.0m, null },
                    { 65, 72000m, 260, 18.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tempeh tươi ngon, giàu protein thực vật. Món ăn vegan, ít calo.", null, 12.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Tempeh\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt chia\",\"Rau thơm\"]", true, "Salad Tempeh", 22.0m, null },
                    { 66, 68000m, 360, 50.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Đậu hũ sốt cà chua đậm đà, kèm cơm. Món ăn vegan, giàu protein thực vật.", null, 10.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Đậu hũ\",\"Cà chua\",\"Hành tây\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", true, "Cơm Đậu Hũ Sốt Cà Chua", 15.0m, null },
                    { 67, 73000m, 340, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl đậu gà với rau củ, giàu protein và chất xơ. Món ăn vegan bổ dưỡng.", null, 11.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Đậu gà\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Sốt tahini\",\"Chanh\",\"Dầu oliu\"]", true, "Bowl Đậu Gà Rau Củ", 18.0m, null },
                    { 68, 75000m, 300, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad quả bơ và đậu hũ, giàu chất béo tốt và protein thực vật. Món ăn vegan.", null, 16.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Đậu hũ\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt hướng dương\"]", true, "Salad Quả Bơ Đậu Hũ", 14.0m, null },
                    { 69, 70000m, 370, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Đậu hũ nướng thơm lừng, kèm cơm và rau củ. Món ăn vegan, giàu protein thực vật.", null, 13.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Đậu hũ\",\"Gia vị nướng\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Dầu oliu\"]", true, "Cơm Đậu Hũ Nướng", 17.0m, null },
                    { 70, 78000m, 330, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl seitan với rau củ, giàu protein thực vật. Món ăn vegan, hương vị đậm đà.", null, 9.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Seitan\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Sốt teriyaki\",\"Dầu oliu\"]", true, "Bowl Seitan Rau Củ", 25.0m, null },
                    { 71, 60000m, 380, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Trứng chiên thơm ngon, kèm cơm và rau củ. Món ăn vegetarian, giàu protein.", null, 14.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Trứng\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Dầu ăn\",\"Nước mắm\"]", true, "Cơm Trứng Chiên", 20.0m, null },
                    { 72, 68000m, 350, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Đậu hũ sốt nấm thơm ngon, kèm cơm. Món ăn vegetarian, giàu protein thực vật.", null, 11.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Đậu hũ\",\"Nấm\",\"Hành tây\",\"Tỏi\",\"Gừng\",\"Gạo\",\"Rau củ\",\"Nước tương\"]", true, "Cơm Đậu Hũ Sốt Nấm", 16.0m, null },
                    { 73, 72000m, 320, 20.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad trứng và quả bơ, giàu protein và chất béo tốt. Món ăn vegetarian.", null, 18.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Trứng\",\"Quả bơ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Hành tây\",\"Dầu giấm\",\"Hạt chia\"]", true, "Salad Trứng Quả Bơ", 16.0m, null },
                    { 74, 75000m, 420, 50.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cơm phô mai nướng thơm lừng, kèm rau củ. Món ăn vegetarian, giàu protein và canxi.", null, 16.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Phô mai\",\"Gạo\",\"Rau củ\",\"Bơ\",\"Tỏi\",\"Gia vị\"]", true, "Cơm Phô Mai Nướng", 18.0m, null },
                    { 75, 78000m, 360, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl trứng và quinoa với rau củ, giàu protein. Món ăn vegetarian bổ dưỡng.", null, 13.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Trứng\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Sốt tahini\",\"Chanh\"]", true, "Bowl Trứng Quinoa", 19.0m, null },
                    { 76, 70000m, 340, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Đậu hũ xào rau củ tươi ngon, kèm cơm. Món ăn vegetarian, giàu protein thực vật.", null, 10.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Đậu hũ\",\"Cà rốt\",\"Ớt chuông\",\"Bông cải xanh\",\"Nấm\",\"Tỏi\",\"Gạo\",\"Nước tương\"]", true, "Cơm Đậu Hũ Xào Rau Củ", 15.0m, null },
                    { 77, 72000m, 290, 18.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad trứng và đậu hũ, giàu protein. Món ăn vegetarian, ít calo.", null, 15.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Trứng\",\"Đậu hũ\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Dầu giấm\",\"Hạt hướng dương\"]", true, "Salad Trứng Đậu Hũ", 20.0m, null },
                    { 78, 62000m, 370, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Trứng ốp la thơm ngon, kèm cơm và rau củ. Món ăn vegetarian, giàu protein.", null, 15.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Trứng\",\"Hành lá\",\"Gạo\",\"Rau củ\",\"Dầu ăn\",\"Nước mắm\",\"Tiêu\"]", true, "Cơm Trứng Ốp La", 19.0m, null },
                    { 79, 75000m, 350, 48.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl đậu hũ và quinoa với rau củ, giàu protein thực vật. Món ăn vegetarian.", null, 12.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Đậu hũ\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Sốt tahini\",\"Chanh\",\"Dầu oliu\"]", true, "Bowl Đậu Hũ Quinoa", 17.0m, null },
                    { 80, 70000m, 390, 50.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Đậu hũ chiên xù giòn tan, kèm cơm và rau củ. Món ăn vegetarian, giòn ngon.", null, 14.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Đậu hũ\",\"Bột chiên xù\",\"Gạo\",\"Rau củ\",\"Nước tương\",\"Dầu ăn\",\"Hành lá\"]", true, "Cơm Đậu Hũ Chiên Xù", 16.0m, null },
                    { 81, 85000m, 320, 8.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad gà keto, ít carb, giàu protein và chất béo tốt. Phù hợp cho chế độ low-carb/keto.", null, 18.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ức gà\",\"Xà lách\",\"Cà chua\",\"Dưa chuột\",\"Quả bơ\",\"Dầu oliu\",\"Chanh\",\"Hạt chia\"]", true, "Salad Gà Keto", 35.0m, null },
                    { 82, 120000m, 380, 10.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá hồi với rau củ keto, ít carb, giàu Omega-3. Phù hợp cho chế độ low-carb/keto.", null, 24.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá hồi\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]", true, "Cá Hồi Rau Củ Keto", 36.0m, null },
                    { 83, 98000m, 350, 12.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò xào rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", null, 18.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Bông cải xanh\",\"Ớt chuông\",\"Nấm\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]", true, "Thịt Bò Xào Rau Củ Keto", 38.0m, null },
                    { 84, 88000m, 330, 9.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nướng với rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", null, 16.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Ức gà\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Gia vị nướng\"]", true, "Gà Nướng Rau Củ Keto", 40.0m, null },
                    { 85, 95000m, 300, 7.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad cá ngừ keto, ít carb, giàu protein và Omega-3. Phù hợp cho chế độ low-carb/keto.", null, 16.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Cá ngừ\",\"Xà lách\",\"Quả bơ\",\"Dưa chuột\",\"Dầu oliu\",\"Chanh\",\"Hạt chia\"]", true, "Salad Cá Ngừ Keto", 32.0m, null },
                    { 86, 105000m, 360, 11.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò nướng với rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", null, 19.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Gia vị nướng\"]", true, "Thịt Bò Nướng Rau Củ Keto", 39.0m, null },
                    { 87, 90000m, 310, 8.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá hấp với rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", null, 15.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Bông cải xanh\",\"Cà rốt\",\"Bơ\",\"Dầu oliu\",\"Gừng\",\"Chanh\"]", true, "Cá Hấp Rau Củ Keto", 34.0m, null },
                    { 88, 82000m, 320, 10.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà xào rau củ keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", null, 14.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Ức gà\",\"Bông cải xanh\",\"Ớt chuông\",\"Nấm\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]", true, "Gà Xào Rau Củ Keto", 36.0m, null },
                    { 89, 110000m, 280, 6.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tôm keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", null, 14.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm\",\"Xà lách\",\"Quả bơ\",\"Dưa chuột\",\"Dầu oliu\",\"Chanh\",\"Hạt chia\"]", true, "Salad Tôm Keto", 28.0m, null },
                    { 90, 100000m, 340, 9.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò xào nấm keto, ít carb, giàu protein. Phù hợp cho chế độ low-carb/keto.", null, 17.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Nấm\",\"Bông cải xanh\",\"Tỏi\",\"Dầu oliu\",\"Gia vị\"]", true, "Thịt Bò Xào Nấm Keto", 37.0m, null },
                    { 91, 88000m, 420, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cơm gà halal thơm ngon, kèm rau củ. Món ăn halal, giàu protein.", null, 14.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà halal\",\"Gạo\",\"Rau củ\",\"Gia vị halal\",\"Dầu oliu\",\"Hành tây\"]", true, "Cơm Gà Halal", 38.0m, null },
                    { 92, 115000m, 460, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cơm thịt bò halal đậm đà, kèm rau củ. Món ăn halal, giàu protein và sắt.", null, 18.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò halal\",\"Gạo\",\"Rau củ\",\"Gia vị halal\",\"Dầu oliu\",\"Tỏi\"]", true, "Cơm Thịt Bò Halal", 40.0m, null },
                    { 93, 92000m, 440, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà nướng halal thơm lừng, kèm cơm và rau củ. Món ăn halal, giàu protein.", null, 16.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà halal\",\"Gia vị nướng halal\",\"Gạo\",\"Rau củ\",\"Dầu oliu\",\"Chanh\"]", true, "Cơm Gà Nướng Halal", 39.0m, null },
                    { 94, 125000m, 480, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cơm thịt cừu halal đậm đà, kèm rau củ. Món ăn halal, giàu protein.", null, 22.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt cừu halal\",\"Gạo\",\"Rau củ\",\"Gia vị halal\",\"Dầu oliu\",\"Hành tây\"]", true, "Cơm Thịt Cừu Halal", 42.0m, null },
                    { 95, 85000m, 400, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà xào halal thơm ngon, kèm cơm. Món ăn halal, giàu protein.", null, 15.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà halal\",\"Hành tây\",\"Ớt chuông\",\"Nấm\",\"Gạo\",\"Gia vị halal\",\"Dầu oliu\"]", true, "Cơm Gà Xào Halal", 36.0m, null },
                    { 96, 88000m, 410, 46.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cơm gạo lứt với gà nướng, không chứa gluten. Món ăn gluten-free, giàu chất xơ.", null, 13.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà\",\"Gạo lứt\",\"Rau củ\",\"Dầu oliu\",\"Gia vị\",\"Chanh\"]", true, "Cơm Gạo Lứt Gà Nướng", 37.0m, null },
                    { 97, 118000m, 390, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl quinoa với cá hồi, không chứa gluten. Món ăn gluten-free, giàu Omega-3.", null, 16.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Cá hồi\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]", true, "Bowl Quinoa Cá Hồi", 34.0m, null },
                    { 98, 108000m, 450, 44.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cơm gạo lứt với thịt bò, không chứa gluten. Món ăn gluten-free, giàu protein.", null, 17.0m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Gạo lứt\",\"Rau củ\",\"Dầu oliu\",\"Gia vị\",\"Tỏi\"]", true, "Cơm Gạo Lứt Thịt Bò", 38.0m, null },
                    { 99, 92000m, 400, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl quinoa với gà, không chứa gluten. Món ăn gluten-free, giàu protein và chất xơ.", null, 14.0m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Gà\",\"Quinoa\",\"Bông cải xanh\",\"Cà rốt\",\"Ớt chuông\",\"Dầu oliu\",\"Gia vị\"]", true, "Bowl Quinoa Gà", 36.0m, null },
                    { 100, 90000m, 380, 40.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cơm gạo lứt với cá nướng, không chứa gluten. Món ăn gluten-free, giàu protein.", null, 12.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá\",\"Gạo lứt\",\"Rau củ\",\"Dầu oliu\",\"Chanh\",\"Gia vị\"]", true, "Cơm Gạo Lứt Cá Nướng", 32.0m, null }
                });

            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "BasePrice", "CreatedAt", "Description", "DurationDays", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, 300000m, new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Gói đăng ký theo tuần (7 ngày)", 7, true, "Weekly" },
                    { 2, 1000000m, new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), "Gói đăng ký theo tháng (30 ngày)", 30, true, "Monthly" }
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
                    { new Guid("44444444-4444-4444-4444-444444444444"), 28, "", new DateTime(2026, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), "user@mealprep.com", "Nguyễn Văn A", 1, true, null, "$2a$11$5HBUykQQvHJfJ9fXFDIqxu6zsTjZcSVnf4SFXQAoSQl4h.UFmaTd2", "0907654321", new Guid("22222222-2222-2222-2222-222222222222") }
                });

            migrationBuilder.InsertData(
                table: "UserNutritionProfiles",
                columns: new[] { "Id", "ActivityLevel", "AppUserId", "CaloriesInDay", "DietPreference", "Goal", "HeightCm", "MealsPerDay", "Notes", "WeightKg" },
                values: new object[] { 1, 3, new Guid("44444444-4444-4444-4444-444444444444"), null, 0, 2, 175, 3, "Tập gym 3-4 lần/tuần, muốn tăng cơ, thích ăn ngọt nhiều và cay ít", 75.5m });

            migrationBuilder.InsertData(
                table: "UserAllergies",
                columns: new[] { "Id", "AllergyName", "UserNutritionProfileId" },
                values: new object[,]
                {
                    { 1, "Đậu phộng", 1 },
                    { 2, "Hải sản", 1 }
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
