using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MealPrep.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModel : Migration
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                columns: new[] { "Id", "BasePrice", "Calories", "Carbs", "CreatedAt", "Description", "Fat", "Images", "Ingredients", "IsActive", "Name", "Protein", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 85000m, 320, 18.2m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ức gà nướng thơm lừng với sốt mật ong đậm đà, kèm rau củ tươi ngon. Món ăn giàu protein, ít calo, phù hợp cho chế độ ăn kiêng và tập luyện.", 12.8m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\",\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Ức gà\",\"Mật ong\",\"Tỏi\",\"Gừng\",\"Hành tây\",\"Dầu oliu\",\"Muối\",\"Tiêu\"]", true, "Gà Nướng Mật Ong", 35.5m, null },
                    { 2, 120000m, 450, 25.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá hồi tươi ngon áp chảo với lớp da giòn tan, kèm rau củ hấp. Nguồn Omega-3 dồi dào, tốt cho tim mạch và não bộ.", 22.5m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\",\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá hồi\",\"Bơ\",\"Chanh\",\"Thì là\",\"Khoai tây\",\"Bông cải xanh\",\"Muối\",\"Tiêu\"]", true, "Cá Hồi Áp Chảo", 38.0m, null },
                    { 3, 75000m, 280, 15.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Salad tươi ngon với ức gà nướng, rau xanh giòn, phô mai feta và hạt óc chó. Món ăn nhẹ, giàu chất xơ và vitamin.", 14.0m, "[\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\",\"https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=800\"]", "[\"Ức gà\",\"Xà lách\",\"Cà chua bi\",\"Dưa chuột\",\"Ớt chuông\",\"Dầu giấm\",\"Phô mai feta\",\"Hạt óc chó\"]", true, "Salad Gà Nướng", 28.0m, null },
                    { 4, 95000m, 380, 22.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Thịt bò mềm xào với rau củ tươi, sốt đậm đà. Món ăn giàu sắt và protein, phù hợp cho người tập gym.", 18.5m, "[\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\",\"https://images.unsplash.com/photo-1558030006-450675393462?w=800\"]", "[\"Thịt bò\",\"Cà rốt\",\"Ớt chuông\",\"Hành tây\",\"Nấm\",\"Tỏi\",\"Gừng\",\"Nước tương\",\"Dầu mè\"]", true, "Bò Xào Rau Củ", 32.0m, null },
                    { 5, 88000m, 420, 45.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Gà tây nướng thơm lừng với cơm gạo lứt và đậu xanh. Món ăn cân bằng dinh dưỡng, giàu protein và chất xơ.", 10.0m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\",\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Gà tây\",\"Gạo lứt\",\"Đậu xanh\",\"Cà rốt\",\"Hành tây\",\"Tỏi\",\"Gia vị nướng\",\"Dầu oliu\"]", true, "Cơm Gà Tây Nướng", 40.0m, null },
                    { 6, 110000m, 250, 20.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tôm tươi sốt cam chua ngọt, kèm rau củ. Món ăn giàu protein, ít calo, phù hợp cho chế độ ăn kiêng.", 8.0m, "[\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\",\"https://images.unsplash.com/photo-1559339352-11d035aa65de?w=800\"]", "[\"Tôm tươi\",\"Cam\",\"Mật ong\",\"Gừng\",\"Tỏi\",\"Ớt\",\"Hành lá\",\"Dầu oliu\",\"Muối\"]", true, "Tôm Sốt Cam", 30.0m, null },
                    { 7, 70000m, 320, 38.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cháo yến mạch ấm nóng với thịt bò băm nhuyễn, rau củ. Món ăn dễ tiêu, giàu chất xơ và protein.", 9.5m, "[\"https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=800\",\"https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=800\"]", "[\"Yến mạch\",\"Thịt bò băm\",\"Hành tây\",\"Cà rốt\",\"Nấm\",\"Hành lá\",\"Gừng\",\"Nước dùng\",\"Muối\"]", true, "Cháo Yến Mạch Thịt Bò", 25.0m, null },
                    { 8, 90000m, 350, 28.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Ức gà sốt teriyaki đậm đà kiểu Nhật, kèm rau củ xào. Món ăn giàu protein, ít béo, phù hợp cho người tập luyện.", 10.5m, "[\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\",\"https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800\"]", "[\"Ức gà\",\"Nước tương\",\"Mật ong\",\"Gừng\",\"Tỏi\",\"Hành tây\",\"Ớt chuông\",\"Dầu mè\",\"Hạt mè\"]", true, "Ức Gà Sốt Teriyaki", 38.0m, null },
                    { 9, 105000m, 380, 30.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cá ngừ tươi nướng vừa chín tới, kèm khoai lang và rau củ. Nguồn protein và Omega-3 dồi dào.", 12.0m, "[\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\",\"https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=800\"]", "[\"Cá ngừ\",\"Chanh\",\"Tỏi\",\"Thì là\",\"Khoai lang\",\"Bông cải xanh\",\"Dầu oliu\",\"Muối\",\"Tiêu\"]", true, "Cá Ngừ Nướng", 42.0m, null },
                    { 10, 92000m, 400, 42.0m, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Bowl quinoa đầy đủ dinh dưỡng với ức gà, rau củ tươi và sốt tahini. Món ăn healthy, giàu protein và chất xơ.", 12.5m, "[\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\",\"https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=800\"]", "[\"Quinoa\",\"Ức gà\",\"Bơ\",\"Cà chua\",\"Dưa chuột\",\"Hành tây đỏ\",\"Rau mầm\",\"Sốt tahini\",\"Chanh\"]", true, "Bowl Quinoa Gà", 35.0m, null }
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
                columns: new[] { "Id", "ActivityLevel", "AppUserId", "DietPreference", "Goal", "HeightCm", "MealsPerDay", "Notes", "WeightKg" },
                values: new object[] { 1, 3, new Guid("44444444-4444-4444-4444-444444444444"), 0, 2, 175, 3, "Tập gym 3-4 lần/tuần, muốn tăng cơ", 75.5m });

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
