using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MealPrep.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliverySlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "DeliverySlots",
                columns: new[] { "Id", "Capacity", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, 100, true, "Morning" },
                    { 2, 100, true, "Afternoon" },
                    { 3, 100, true, "Evening" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DeliverySlots",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DeliverySlots",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DeliverySlots",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
