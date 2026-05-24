using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VocabApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelToPredefinedWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "PredefinedWords",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Level",
                table: "PredefinedWords");
        }
    }
}
