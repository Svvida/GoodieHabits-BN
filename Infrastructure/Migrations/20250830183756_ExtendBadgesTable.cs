using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendBadgesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfile_Badges_Badges_BadgeId",
                table: "UserProfile_Badges");

            migrationBuilder.DropIndex(
                name: "IX_UserProfile_Badges_UserProfileId_BadgeId",
                table: "UserProfile_Badges");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Badges");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Badges",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ColorHex",
                table: "Badges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Badges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Badges",
                columns: new[] { "Id", "ColorHex", "Description", "Text", "Type" },
                values: new object[,]
                {
                    { 1, "#008000", "One week of daily quests in a row!", "Daily Streak: 7", "CompleteDailySeven" },
                    { 2, "#008000", "A whole month of consistency. The fire never went out.", "Daily Streak: 30", "CompleteDailyThirty" },
                    { 3, "#008000", "Completed a monthly quest every month for a full year.", "Cycle Breaker", "CompleteMonthlyTwelve" },
                    { 4, "#008000", "500 quests completed. Legendary endurance!", "Quest Master", "Complete500Quests" },
                    { 5, "#ffbf00", "You set your very first goal.", "Dreamer", "GoalCreateFirst" },
                    { 6, "#ffbf00", "You drafted 10 goals to challenge yourself.", "Planner", "GoalCreateTen" },
                    { 7, "#ffbf00", "Your first goal completed. The journey begins.", "Achiever", "GoalCompleteFirst" },
                    { 8, "#ffbf00", "10 goals done. Determination proven.", "Goal Getter", "GoalCompleteTen" },
                    { 9, "#ffbf00", "50 goals achieved. You’ve come a long way.", "Vision Realized", "GoalCompleteFifty" },
                    { 10, "#ffd700", "Completed a yearly goal. True perseverance.", "Yearly Champion", "GoalCompleteYearly" },
                    { 11, "#0000ff", "Your adventure begins with the very first quest.", "First Quest!", "CreateFirstQuest" },
                    { 12, "#0000ff", "Twenty quests written into your story.", "Quest Scribe", "CreateTwentyQuests" },
                    { 13, "#0000ff", "100 quests forged. A true architect of challenges.", "Quest Factory", "Create100Quests" },
                    { 14, "#6a0dad", "Your first companion on the journey.", "Ally Found", "MakeOneFriend" },
                    { 15, "#6a0dad", "Five allies joined your party.", "Band of Allies", "MakeFiveFriends" },
                    { 16, "#6a0dad", "Ten friends along the way.", "Social Starter", "MakeTenFriends" },
                    { 17, "#6a0dad", "Twenty friends — your guild thrives.", "Party Leader", "MakeTwentyFriends" },
                    { 18, "#ff0000", "Failed 10 times, but always rose again.", "Phoenix", "FailureComeback" },
                    { 19, "#ff0000", "Lost your streak, but started again. That’s true grit.", "Gritty Hero", "StreakRecovery" },
                    { 20, "#c0c0c0", "Completed 10 daily, 10 weekly, and 10 monthly quests.", "Balanced Hero", "BalancedHero" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfile_Badges_UserProfileId_EarnedAt",
                table: "UserProfile_Badges",
                columns: new[] { "UserProfileId", "EarnedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfile_Badges_Badges_BadgeId",
                table: "UserProfile_Badges",
                column: "BadgeId",
                principalTable: "Badges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProfile_Badges_Badges_BadgeId",
                table: "UserProfile_Badges");

            migrationBuilder.DropIndex(
                name: "IX_UserProfile_Badges_UserProfileId_EarnedAt",
                table: "UserProfile_Badges");

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Badges",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DropColumn(
                name: "ColorHex",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Badges");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Badges",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Badges",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Badges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfile_Badges_UserProfileId_BadgeId",
                table: "UserProfile_Badges",
                columns: new[] { "UserProfileId", "BadgeId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserProfile_Badges_Badges_BadgeId",
                table: "UserProfile_Badges",
                column: "BadgeId",
                principalTable: "Badges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
