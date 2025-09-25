using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendTrackingOfRepeatableCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletedDailyQuests",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompletedMonthlyQuests",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompletedWeeklyQuests",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE UserProfiles
                SET CompletedDailyQuests = (SELECT COUNT(Id) FROM Quests WHERE QuestType = 'Daily' AND IsCompleted = 1 AND AccountId = UserProfiles.AccountId);");

            migrationBuilder.Sql(@"
                UPDATE UserProfiles
                SET CompletedWeeklyQuests = (SELECT COUNT(Id) FROM Quests WHERE QuestType = 'Weekly' AND IsCompleted = 1 AND AccountId = UserProfiles.AccountId);");

            migrationBuilder.Sql(@"
                UPDATE UserProfiles
                SET CompletedMonthlyQuests = (SELECT COUNT(Id) FROM Quests WHERE QuestType = 'Monthly' AND IsCompleted = 1 AND AccountId = UserProfiles.AccountId);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedDailyQuests",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CompletedMonthlyQuests",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CompletedWeeklyQuests",
                table: "UserProfiles");
        }
    }
}
