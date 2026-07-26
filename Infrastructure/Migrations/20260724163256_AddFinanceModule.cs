using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "UserProfiles",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.CreateTable(
                name: "FinanceCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: true),
                    ParentCategoryId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsSavings = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceCategories_FinanceCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "FinanceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinanceCategories_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Budgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    Period = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: true),
                    LimitAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Budgets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Budgets_FinanceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "FinanceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Budgets_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FinanceTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    OccurredOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinanceTransactions_FinanceCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "FinanceCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinanceTransactions_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "FinanceCategories",
                columns: new[] { "Id", "Color", "CreatedAt", "Icon", "IsSystem", "Name", "ParentCategoryId", "Type", "UpdatedAt", "UserProfileId" },
                values: new object[,]
                {
                    { 1, "#4E79A7", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "home", true, "Home", null, 1, null, null },
                    { 2, "#F28E2B", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "food", true, "Food", null, 1, null, null },
                    { 3, "#E15759", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "car", true, "Transport", null, 1, null, null },
                    { 4, "#76B7B2", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "health", true, "Health", null, 1, null, null },
                    { 5, "#59A14F", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "entertainment", true, "Entertainment", null, 1, null, null },
                    { 6, "#EDC948", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "shopping", true, "Shopping", null, 1, null, null },
                    { 7, "#B07AA1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "dots", true, "Other", null, 1, null, null },
                    { 50, "#59A14F", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "salary", true, "Salary", null, 0, null, null },
                    { 51, "#8CD17D", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "bonus", true, "Bonus", null, 0, null, null },
                    { 52, "#B6992D", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "gift", true, "Gifts", null, 0, null, null },
                    { 53, "#499894", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "investment", true, "Investments", null, 0, null, null },
                    { 54, "#86BCB6", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "dots", true, "Other", null, 0, null, null },
                    { 101, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Rent", 1, 1, null, null },
                    { 102, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Internet", 1, 1, null, null },
                    { 103, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Electricity", 1, 1, null, null },
                    { 104, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Water", 1, 1, null, null },
                    { 105, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Groceries", 2, 1, null, null },
                    { 106, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Restaurants", 2, 1, null, null },
                    { 107, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Fuel", 3, 1, null, null },
                    { 108, null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, "Public transport", 3, 1, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CategoryId",
                table: "Budgets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_UserProfileId_CategoryId_Period_Year_Month",
                table: "Budgets",
                columns: new[] { "UserProfileId", "CategoryId", "Period", "Year", "Month" },
                unique: true,
                filter: "[CategoryId] IS NOT NULL AND [Month] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceCategories_ParentCategoryId",
                table: "FinanceCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceCategories_UserProfileId_ParentCategoryId",
                table: "FinanceCategories",
                columns: new[] { "UserProfileId", "ParentCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_FinanceTransactions_CategoryId",
                table: "FinanceTransactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FinanceTransactions_UserProfileId_CategoryId_OccurredOn",
                table: "FinanceTransactions",
                columns: new[] { "UserProfileId", "CategoryId", "OccurredOn" });

            migrationBuilder.CreateIndex(
                name: "IX_FinanceTransactions_UserProfileId_OccurredOn",
                table: "FinanceTransactions",
                columns: new[] { "UserProfileId", "OccurredOn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Budgets");

            migrationBuilder.DropTable(
                name: "FinanceTransactions");

            migrationBuilder.DropTable(
                name: "FinanceCategories");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "UserProfiles");
        }
    }
}
