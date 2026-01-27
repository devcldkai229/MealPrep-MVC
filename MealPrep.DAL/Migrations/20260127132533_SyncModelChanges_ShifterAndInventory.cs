using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPrep.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges_ShifterAndInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShipperId",
                table: "DeliveryOrders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrders_ShipperId",
                table: "DeliveryOrders",
                column: "ShipperId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryOrders_Users_ShipperId",
                table: "DeliveryOrders",
                column: "ShipperId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryOrders_Users_ShipperId",
                table: "DeliveryOrders");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryOrders_ShipperId",
                table: "DeliveryOrders");

            migrationBuilder.DropColumn(
                name: "ShipperId",
                table: "DeliveryOrders");
        }
    }
}
