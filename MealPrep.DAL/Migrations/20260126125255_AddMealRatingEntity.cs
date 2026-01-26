using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPrep.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMealRatingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MealRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryOrderItemId = table.Column<int>(type: "int", nullable: false),
                    MealId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Stars = table.Column<int>(type: "int", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RequestedBlock = table.Column<bool>(type: "bit", nullable: false),
                    MarkedAsConsumed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealRatings_DeliveryOrderItems_DeliveryOrderItemId",
                        column: x => x.DeliveryOrderItemId,
                        principalTable: "DeliveryOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealRatings_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MealRatings_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MealRatings_AppUserId_DeliveryOrderItemId",
                table: "MealRatings",
                columns: new[] { "AppUserId", "DeliveryOrderItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealRatings_DeliveryOrderItemId",
                table: "MealRatings",
                column: "DeliveryOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MealRatings_MealId",
                table: "MealRatings",
                column: "MealId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealRatings");
        }
    }
}
