using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeZillion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldToWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Field",
                table: "Words",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Words_OwnerId_Field",
                table: "Words",
                columns: new[] { "OwnerId", "Field" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Words_OwnerId_Field",
                table: "Words");

            migrationBuilder.DropColumn(
                name: "Field",
                table: "Words");
        }
    }
}

