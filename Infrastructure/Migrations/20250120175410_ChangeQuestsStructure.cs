using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeQuestsStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recurring_Quests");

            migrationBuilder.DropColumn(
                name: "IsImportant",
                table: "Seasonal_Quests");

            migrationBuilder.DropColumn(
                name: "IsImportant",
                table: "One_Time_Quests");

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Seasonal_Quests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "One_Time_Quests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Repeatable_Quests",
                columns: table => new
                {
                    RepeatableQuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RepeatTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    RepeatInterval = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repeatable_Quests", x => x.RepeatableQuestId);
                    table.ForeignKey(
                        name: "FK_Repeatable_Quests_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Repeatable_Quests_AccountId",
                table: "Repeatable_Quests",
                column: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Repeatable_Quests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Seasonal_Quests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "One_Time_Quests");

            migrationBuilder.AddColumn<bool>(
                name: "IsImportant",
                table: "Seasonal_Quests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsImportant",
                table: "One_Time_Quests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Recurring_Quests",
                columns: table => new
                {
                    RecurringQuestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsImportant = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RepeatIntervalJson = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    RepeatTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recurring_Quests", x => x.RecurringQuestId);
                    table.ForeignKey(
                        name: "FK_Recurring_Quests_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recurring_Quests_AccountId",
                table: "Recurring_Quests",
                column: "AccountId");
        }
    }
}
