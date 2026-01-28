using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPrep.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddKitchenInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KitchenInventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MealId = table.Column<int>(type: "int", nullable: false),
                    QuantityLimit = table.Column<int>(type: "int", nullable: false),
                    QuantityUsed = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KitchenInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KitchenInventories_Meals_MealId",
                        column: x => x.MealId,
                        principalTable: "Meals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KitchenInventories_Date_MealId",
                table: "KitchenInventories",
                columns: new[] { "Date", "MealId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KitchenInventories_MealId",
                table: "KitchenInventories",
                column: "MealId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KitchenInventories");
        }
    }
}
