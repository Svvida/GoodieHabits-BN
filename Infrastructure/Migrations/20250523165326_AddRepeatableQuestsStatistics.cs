using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRepeatableQuestsStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGoals_Accounts_AccountId",
                table: "UserGoals");

            migrationBuilder.AlterColumn<int>(
                name: "XpBonus",
                table: "UserGoals",
                type: "int",
                nullable: false,
                defaultValue: 5,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsExpired",
                table: "UserGoals",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAchieved",
                table: "UserGoals",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserGoals",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateTable(
                name: "QuestOccurrences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    OccurrenceStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OccurrenceEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WasCompleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestOccurrences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestOccurrences_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestStatistics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    CompletionCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FailureCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OccurrenceCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LongestStreak = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestStatistics_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_AccountId_IsAchieved_IsExpired",
                table: "UserGoals",
                columns: new[] { "AccountId", "IsAchieved", "IsExpired" });

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_EndsAt",
                table: "UserGoals",
                column: "EndsAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuestOccurrences_CompletedAt",
                table: "QuestOccurrences",
                column: "CompletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_QuestOccurrences_OccurrenceStart",
                table: "QuestOccurrences",
                column: "OccurrenceStart");

            migrationBuilder.CreateIndex(
                name: "IX_QuestOccurrences_QuestId",
                table: "QuestOccurrences",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestOccurrences_QuestId_OccurrenceStart_OccurrenceEnd",
                table: "QuestOccurrences",
                columns: new[] { "QuestId", "OccurrenceStart", "OccurrenceEnd" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestStatistics_QuestId",
                table: "QuestStatistics",
                column: "QuestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGoals_Accounts_AccountId",
                table: "UserGoals",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGoals_Accounts_AccountId",
                table: "UserGoals");

            migrationBuilder.DropTable(
                name: "QuestOccurrences");

            migrationBuilder.DropTable(
                name: "QuestStatistics");

            migrationBuilder.DropIndex(
                name: "IX_UserGoals_AccountId_IsAchieved_IsExpired",
                table: "UserGoals");

            migrationBuilder.DropIndex(
                name: "IX_UserGoals_EndsAt",
                table: "UserGoals");

            migrationBuilder.AlterColumn<int>(
                name: "XpBonus",
                table: "UserGoals",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 5);

            migrationBuilder.AlterColumn<bool>(
                name: "IsExpired",
                table: "UserGoals",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsAchieved",
                table: "UserGoals",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "UserGoals",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_UserGoals_Accounts_AccountId",
                table: "UserGoals",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
