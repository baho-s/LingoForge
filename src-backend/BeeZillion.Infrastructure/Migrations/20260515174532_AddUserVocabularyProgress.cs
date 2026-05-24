using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BeeZillion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserVocabularyProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("259b50bf-7c68-42f9-a671-64ab6b1b2281"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("2eb089a7-4d99-4778-bde9-35ea32e46939"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("3eb376f5-3412-4534-b342-03e3e13cab3a"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("454b4275-6c87-4c55-ad76-0837d44993a5"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("4a5f9438-b2a4-46be-8be7-5cb8c775309d"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("5d090354-b672-4635-a76d-93a01e782b29"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("8c2a3f5e-da7b-4052-ac2e-b73bc1bd7b10"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("9518b5d7-3aea-4c4b-b001-969cef8b84ce"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("a985bcd6-f85f-42c2-b4fc-fe5a80a32490"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("c207887d-8c34-4b75-9065-c0f759e0bc50"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("d8bc78d6-767c-44b1-9c49-8c8ea0d70983"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("efa5d740-577a-4b2b-9e9b-b1926cdc9d8d"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("f9eec26d-4ad1-4788-82e3-e9e060ec81d8"));

            migrationBuilder.CreateTable(
                name: "UserVocabularyProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalAttempts = table.Column<int>(type: "int", nullable: false),
                    CorrectAttempts = table.Column<int>(type: "int", nullable: false),
                    AverageTimeTakenMs = table.Column<long>(type: "bigint", nullable: false),
                    MinTimeTakenMs = table.Column<long>(type: "bigint", nullable: false),
                    MaxTimeTakenMs = table.Column<long>(type: "bigint", nullable: false),
                    LastSelectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsecutiveSelections = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVocabularyProgresses", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PredefinedWords",
                columns: new[] { "Id", "AiSentence", "Category", "CreatedAt", "Field", "IsActive", "Original", "Translation" },
                values: new object[,]
                {
                    { new Guid("02275182-c135-474d-9403-5f7c77716e5c"), "A framework provides a foundation for building applications.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2486), "Software", true, "framework", "framework" },
                    { new Guid("061be113-13f2-40eb-9c19-e405cc215e4a"), "A defendant is a person accused of a crime.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2525), "Law", true, "defendant", "davalý" },
                    { new Guid("0c3cc2b1-0633-46d6-bd62-3d72d92d2988"), "To debug means to find and fix errors in code.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2483), "Software", true, "debug", "hata ayýklamak" },
                    { new Guid("229e63c8-ef36-4b58-beed-498dd9b32e58"), "A symptom is a sign of illness or disease.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2512), "Medicine", true, "symptom", "semptom" },
                    { new Guid("356055e5-e47d-4d71-b0bf-b18e543126ba"), "A repository is a central storage location for code.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2490), "Software", true, "repository", "depo" },
                    { new Guid("6a53fbf4-61b6-4f88-a842-f0504cbb8dff"), "Prognosis is the likely outcome of a disease.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2509), "Medicine", true, "prognosis", "hastalýk gidiþi" },
                    { new Guid("8828ce07-34f7-485c-b4ff-0317e3b67e63"), "An attorney is a lawyer who represents clients.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2536), "Law", true, "attorney", "avukat" },
                    { new Guid("8b2e0123-1b72-4b21-be5f-8f223730103e"), "A lawsuit is a legal action brought in court.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2532), "Law", true, "lawsuit", "dava" },
                    { new Guid("bca7af0e-6048-4038-a35a-023335c0bee8"), "An algorithm is a step-by-step procedure for solving a problem.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2466), "Software", true, "algorithm", "algoritma" },
                    { new Guid("c38d48ab-e002-4283-b736-98f7c28c9f01"), "A verdict is the decision made by a court.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2529), "Law", true, "verdict", "karar" },
                    { new Guid("cf44c157-53ad-48b8-81c7-5e5e53bc9374"), "A bug is an error or flaw in a software program.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2479), "Software", true, "bug", "hata" },
                    { new Guid("e42922af-d212-4459-a317-73c5a806aad1"), "Treatment is the medical care given for an illness.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2520), "Medicine", true, "treatment", "tedavi" },
                    { new Guid("ed9e1d2f-12f4-45bb-84e0-7cc6252fdb30"), "Diagnosis is the identification of a disease or condition.", "General", new DateTime(2026, 5, 15, 17, 45, 31, 173, DateTimeKind.Utc).AddTicks(2504), "Medicine", true, "diagnosis", "taný" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyProgresses_UserId_LastSelectedAt",
                table: "UserVocabularyProgresses",
                columns: new[] { "UserId", "LastSelectedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyProgresses_UserId_UpdatedAt",
                table: "UserVocabularyProgresses",
                columns: new[] { "UserId", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserVocabularyProgresses_UserId_WordId",
                table: "UserVocabularyProgresses",
                columns: new[] { "UserId", "WordId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserVocabularyProgresses");

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("02275182-c135-474d-9403-5f7c77716e5c"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("061be113-13f2-40eb-9c19-e405cc215e4a"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("0c3cc2b1-0633-46d6-bd62-3d72d92d2988"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("229e63c8-ef36-4b58-beed-498dd9b32e58"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("356055e5-e47d-4d71-b0bf-b18e543126ba"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("6a53fbf4-61b6-4f88-a842-f0504cbb8dff"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("8828ce07-34f7-485c-b4ff-0317e3b67e63"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("8b2e0123-1b72-4b21-be5f-8f223730103e"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("bca7af0e-6048-4038-a35a-023335c0bee8"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("c38d48ab-e002-4283-b736-98f7c28c9f01"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("cf44c157-53ad-48b8-81c7-5e5e53bc9374"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("e42922af-d212-4459-a317-73c5a806aad1"));

            migrationBuilder.DeleteData(
                table: "PredefinedWords",
                keyColumn: "Id",
                keyValue: new Guid("ed9e1d2f-12f4-45bb-84e0-7cc6252fdb30"));

            migrationBuilder.InsertData(
                table: "PredefinedWords",
                columns: new[] { "Id", "AiSentence", "Category", "CreatedAt", "Field", "IsActive", "Original", "Translation" },
                values: new object[,]
                {
                    { new Guid("259b50bf-7c68-42f9-a671-64ab6b1b2281"), "A repository is a central storage location for code.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5913), "Software", true, "repository", "depo" },
                    { new Guid("2eb089a7-4d99-4778-bde9-35ea32e46939"), "An attorney is a lawyer who represents clients.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5991), "Law", true, "attorney", "avukat" },
                    { new Guid("3eb376f5-3412-4534-b342-03e3e13cab3a"), "A symptom is a sign of illness or disease.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5965), "Medicine", true, "symptom", "semptom" },
                    { new Guid("454b4275-6c87-4c55-ad76-0837d44993a5"), "To debug means to find and fix errors in code.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5904), "Software", true, "debug", "hata ayýklamak" },
                    { new Guid("4a5f9438-b2a4-46be-8be7-5cb8c775309d"), "Treatment is the medical care given for an illness.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5969), "Medicine", true, "treatment", "tedavi" },
                    { new Guid("5d090354-b672-4635-a76d-93a01e782b29"), "Diagnosis is the identification of a disease or condition.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5937), "Medicine", true, "diagnosis", "taný" },
                    { new Guid("8c2a3f5e-da7b-4052-ac2e-b73bc1bd7b10"), "A framework provides a foundation for building applications.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5909), "Software", true, "framework", "framework" },
                    { new Guid("9518b5d7-3aea-4c4b-b001-969cef8b84ce"), "A verdict is the decision made by a court.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5981), "Law", true, "verdict", "karar" },
                    { new Guid("a985bcd6-f85f-42c2-b4fc-fe5a80a32490"), "Prognosis is the likely outcome of a disease.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5960), "Medicine", true, "prognosis", "hastalýk gidiþi" },
                    { new Guid("c207887d-8c34-4b75-9065-c0f759e0bc50"), "A lawsuit is a legal action brought in court.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5986), "Law", true, "lawsuit", "dava" },
                    { new Guid("d8bc78d6-767c-44b1-9c49-8c8ea0d70983"), "A defendant is a person accused of a crime.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5976), "Law", true, "defendant", "davalý" },
                    { new Guid("efa5d740-577a-4b2b-9e9b-b1926cdc9d8d"), "A bug is an error or flaw in a software program.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5899), "Software", true, "bug", "hata" },
                    { new Guid("f9eec26d-4ad1-4788-82e3-e9e060ec81d8"), "An algorithm is a step-by-step procedure for solving a problem.", "General", new DateTime(2026, 5, 14, 20, 20, 46, 634, DateTimeKind.Utc).AddTicks(5881), "Software", true, "algorithm", "algoritma" }
                });
        }
    }
}

