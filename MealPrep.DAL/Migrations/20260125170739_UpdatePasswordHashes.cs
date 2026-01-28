using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MealPrep.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePasswordHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$BgEVIOqTroolit.QXo8ZVedAuTsoAevkQVuyc/AhK02D9iDFQvimu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "PasswordHash",
                value: "$2a$11$w6EYO9JNYHx7EsbDMmv85.9akoeiDFVXsAmfmTn3BXtY1mueukRcG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$pxXoT3Q7rI/BJC7WrofzouutZ/Fa/zkmdCiv3yTMRC6Cx47v0uPXe");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "PasswordHash",
                value: "$2a$11$5HBUykQQvHJfJ9fXFDIqxu6zsTjZcSVnf4SFXQAoSQl4h.UFmaTd2");
        }
    }
}
