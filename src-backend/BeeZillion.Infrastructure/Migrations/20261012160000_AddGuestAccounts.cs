using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeZillion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AiUsageCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "GuestExpiresAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGuest",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiUsageCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GuestExpiresAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsGuest",
                table: "Users");
        }
    }
}
