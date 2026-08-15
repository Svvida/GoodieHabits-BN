using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PromoteDebtRepaymentToMainCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 147,
                columns: new[] { "Color", "Icon", "IsSavings", "ParentCategoryId" },
                values: new object[] { "#EF4444", "arrow-undo-outline", false, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "FinanceCategories",
                keyColumn: "Id",
                keyValue: 147,
                columns: new[] { "Color", "Icon", "IsSavings", "ParentCategoryId" },
                values: new object[] { null, null, true, 6 });
        }
    }
}
