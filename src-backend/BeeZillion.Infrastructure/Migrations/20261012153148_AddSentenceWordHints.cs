using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeeZillion.Infrastructure.Migrations
{
    public partial class AddSentenceWordHints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SentenceWordHints",
                columns: table => new
                {
                    WordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SentenceHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    NormalizedTerm = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Term = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Translation = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_SentenceWordHints",
                        x => new { x.WordId, x.SentenceHash, x.NormalizedTerm });
                    table.ForeignKey(
                        name: "FK_SentenceWordHints_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SentenceWordHints");
        }
    }
}
