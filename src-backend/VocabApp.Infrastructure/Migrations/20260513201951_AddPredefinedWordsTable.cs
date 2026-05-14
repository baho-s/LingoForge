using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VocabApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPredefinedWordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PredefinedWords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Field = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Original = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Translation = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AiSentence = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PredefinedWords", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PredefinedWords",
                columns: new[] { "Id", "AiSentence", "Category", "CreatedAt", "Field", "IsActive", "Original", "Translation" },
                values: new object[,]
                {
                    { new Guid("084e7fe8-13c8-4773-9b85-eb054b6b549a"), "Diagnosis is the identification of a disease or condition.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 346, DateTimeKind.Utc).AddTicks(9976), "Medicine", true, "diagnosis", "tanı" },
                    { new Guid("16b6999a-d974-48f5-9cdd-2a8221be9b39"), "To debug means to find and fix errors in code.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 346, DateTimeKind.Utc).AddTicks(9928), "Software", true, "debug", "hata ayıklamak" },
                    { new Guid("1b7a5571-9388-436e-ae9b-792959c973da"), "A bug is an error or flaw in a software program.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 346, DateTimeKind.Utc).AddTicks(9920), "Software", true, "bug", "hata" },
                    { new Guid("20fd5b19-adb8-49df-9786-b3118ae0c031"), "Prognosis is the likely outcome of a disease.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 346, DateTimeKind.Utc).AddTicks(9983), "Medicine", true, "prognosis", "hastalık gidişi" },
                    { new Guid("2ae720d9-5830-42b8-927d-997cdc5bc95a"), "A lawsuit is a legal action brought in court.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 347, DateTimeKind.Utc).AddTicks(40), "Law", true, "lawsuit", "dava" },
                    { new Guid("5b28f01b-18ec-4431-b1c6-097d6930fc89"), "An algorithm is a step-by-step procedure for solving a problem.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 346, DateTimeKind.Utc).AddTicks(9823), "Software", true, "algorithm", "algoritma" },
                    { new Guid("6287df7b-8098-4954-9289-ad6523e46c35"), "A symptom is a sign of illness or disease.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 347, DateTimeKind.Utc).AddTicks(16), "Medicine", true, "symptom", "semptom" },
                    { new Guid("6f1bcfdf-1fcd-4369-8d91-1c90c9c5b8cf"), "Treatment is the medical care given for an illness.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 347, DateTimeKind.Utc).AddTicks(22), "Medicine", true, "treatment", "tedavi" },
                    { new Guid("72056339-4756-498f-a266-e05d77613a77"), "A framework provides a foundation for building applications.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 346, DateTimeKind.Utc).AddTicks(9936), "Software", true, "framework", "framework" },
                    { new Guid("72d2cf05-3dce-4b16-b125-ada4365580ac"), "A defendant is a person accused of a crime.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 347, DateTimeKind.Utc).AddTicks(30), "Law", true, "defendant", "davalı" },
                    { new Guid("759719e5-a651-4ad9-a67d-2ae778331e83"), "A verdict is the decision made by a court.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 347, DateTimeKind.Utc).AddTicks(35), "Law", true, "verdict", "karar" },
                    { new Guid("8d8343b9-de16-4113-9cd4-30614122a953"), "An attorney is a lawyer who represents clients.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 347, DateTimeKind.Utc).AddTicks(45), "Law", true, "attorney", "avukat" },
                    { new Guid("a6265f55-78b6-495a-98cd-69588c5c58b3"), "A repository is a central storage location for code.", "General", new DateTime(2026, 5, 13, 20, 19, 50, 346, DateTimeKind.Utc).AddTicks(9942), "Software", true, "repository", "depo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_PredefinedWords_Field_Category",
                table: "PredefinedWords",
                columns: new[] { "Field", "Category" });

            migrationBuilder.CreateIndex(
                name: "IX_PredefinedWords_Field_IsActive",
                table: "PredefinedWords",
                columns: new[] { "Field", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PredefinedWords_Original",
                table: "PredefinedWords",
                column: "Original");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PredefinedWords");
        }
    }
}
