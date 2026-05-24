using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeZillion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLastReviewedAtToReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewLastReviewedAt",
                table: "Words",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewLastReviewedAt",
                table: "Words");
        }
    }
}

