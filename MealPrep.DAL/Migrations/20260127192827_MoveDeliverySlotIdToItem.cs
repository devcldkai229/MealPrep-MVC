using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPrep.DAL.Migrations
{
    /// <inheritdoc />
    public partial class MoveDeliverySlotIdToItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryOrders_DeliverySlots_DeliverySlotId",
                table: "DeliveryOrders");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryOrders_DeliverySlotId",
                table: "DeliveryOrders");

            migrationBuilder.DropColumn(
                name: "DeliverySlotId",
                table: "DeliveryOrders");

            migrationBuilder.AddColumn<int>(
                name: "DeliverySlotId",
                table: "DeliveryOrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrderItems_DeliverySlotId",
                table: "DeliveryOrderItems",
                column: "DeliverySlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryOrderItems_DeliverySlots_DeliverySlotId",
                table: "DeliveryOrderItems",
                column: "DeliverySlotId",
                principalTable: "DeliverySlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryOrderItems_DeliverySlots_DeliverySlotId",
                table: "DeliveryOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryOrderItems_DeliverySlotId",
                table: "DeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "DeliverySlotId",
                table: "DeliveryOrderItems");

            migrationBuilder.AddColumn<int>(
                name: "DeliverySlotId",
                table: "DeliveryOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryOrders_DeliverySlotId",
                table: "DeliveryOrders",
                column: "DeliverySlotId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryOrders_DeliverySlots_DeliverySlotId",
                table: "DeliveryOrders",
                column: "DeliverySlotId",
                principalTable: "DeliverySlots",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
