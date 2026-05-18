using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VocabApp.Infrastructure.Persistence;

#nullable disable

namespace VocabApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260518170000_AddReviewHistory")]
    public partial class AddReviewHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    Outcome = table.Column<int>(type: "int", nullable: false),
                    QScore = table.Column<int>(type: "int", nullable: true),
                    TimeTakenMs = table.Column<long>(type: "bigint", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextReviewAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IntervalDays = table.Column<int>(type: "int", nullable: false),
                    EaseFactor = table.Column<float>(type: "real", nullable: false),
                    Repetitions = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClientVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHistories_UserId_WordId",
                table: "ReviewHistories",
                columns: new[] { "UserId", "WordId" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHistories_UserId_ReviewedAt",
                table: "ReviewHistories",
                columns: new[] { "UserId", "ReviewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHistories_WordId_ReviewedAt",
                table: "ReviewHistories",
                columns: new[] { "WordId", "ReviewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewHistories_UserId_Outcome",
                table: "ReviewHistories",
                columns: new[] { "UserId", "Outcome" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewHistories");
        }
    }
}
