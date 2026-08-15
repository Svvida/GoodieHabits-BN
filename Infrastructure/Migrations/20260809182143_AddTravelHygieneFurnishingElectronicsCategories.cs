using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelHygieneFurnishingElectronicsCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Seeded system categories and user-created ones share a single IDENTITY sequence, so the counter
            // eventually walks into the range reserved for seed data — it already had: this migration first
            // failed because user rows held 150-153. Reseeding to 100_000 splits the ranges for good (system
            // below, users above). Inserting an explicit id below the counter never lowers it — SQL Server only
            // raises the identity on explicit insert — so the seed rows added afterwards leave it at 100_000.
            migrationBuilder.Sql(
                "IF IDENT_CURRENT('FinanceCategories') < 100000 " +
                "DBCC CHECKIDENT ('FinanceCategories', RESEED, 100000);");

            migrationBuilder.InsertData(
                table: "FinanceCategories",
                columns: new[] { "Id", "Color", "CreatedAt", "Icon", "IsSystem", "Name", "ParentCategoryId", "Type", "UpdatedAt", "UserProfileId" },
                values: new object[,]
                {
                    { 9, "#0EA5E9", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "airplane-outline", true, "Podróże i Wakacje", null, 1, null, null },
                    { 164, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Higiena i kosmetyki", 3, 1, null, null },
                    { 165, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Wyposażenie i meble", 1, 1, null, null },
                    { 166, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Elektronika i sprzęt", 5, 1, null, null },
                    { 158, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Noclegi", 9, 1, null, null },
                    { 159, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Przejazdy / Loty", 9, 1, null, null },
                    { 160, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Wynajem auta", 9, 1, null, null },
                    { 161, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Jedzenie na wyjeździe", 9, 1, null, null },
                    { 162, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Atrakcje i wycieczki", 9, 1, null, null },
                    { 163, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Ubezpieczenie podróżne", 9, 1, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 158);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 159);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 160);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 161);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 162);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 163);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 164);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 165);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 166);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
