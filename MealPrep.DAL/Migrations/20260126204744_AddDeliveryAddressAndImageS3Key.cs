using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPrep.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryAddressAndImageS3Key : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "DeliveryOrderItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryAddress",
                table: "DeliveryOrderItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageS3Key",
                table: "DeliveryOrderItems",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "DeliveryAddress",
                value: null);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "DeliveryAddress",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "DeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "DeliveryAddress",
                table: "DeliveryOrderItems");

            migrationBuilder.DropColumn(
                name: "ImageS3Key",
                table: "DeliveryOrderItems");
        }
    }
}
