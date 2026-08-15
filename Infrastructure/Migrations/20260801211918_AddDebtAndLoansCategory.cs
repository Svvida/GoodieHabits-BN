using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDebtAndLoansCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Order matters: the parent row (8) has to exist before 147 can be re-pointed at it, otherwise the
            // self-referencing FK rejects the update. EF scaffolds the update first, so this is hand-ordered.
            migrationBuilder.InsertData(
                table: "FinanceCategories",
                columns: new[] { "Id", "Color", "CreatedAt", "Icon", "IsSystem", "Name", "ParentCategoryId", "Type", "UpdatedAt", "UserProfileId" },
                values: new object[,]
                {
                    { 8, "#EF4444", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "swap-horizontal-outline", true, "Długi i Pożyczki", null, 1, null, null },
                    { 148, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Pożyczki udzielone", 8, 1, null, null }
                });

            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 147,
                columns: new[] { "Color", "Icon", "ParentCategoryId" },
                values: new object[] { null, null, 8 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Mirror of Up: detach 147 from its parent before deleting 8, or the Restrict FK blocks the delete.
            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 147,
                columns: new[] { "Color", "Icon", "ParentCategoryId" },
                values: new object[] { "#EF4444", "arrow-undo-outline", null });

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 148);

            migrationBuilder.DeleteData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
