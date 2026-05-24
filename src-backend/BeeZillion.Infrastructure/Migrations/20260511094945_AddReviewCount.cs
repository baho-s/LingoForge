using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeZillion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Users");
        }
    }
}

