using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SwapAccountWithProfileForGameThings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quest_Labels_Accounts_AccountId",
                table: "Quest_Labels");

            migrationBuilder.DropForeignKey(
                name: "FK_Quests_Accounts_AccountId",
                table: "Quests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGoals_Accounts_AccountId",
                table: "UserGoals");

            migrationBuilder.DropIndex(
                name: "IX_UserGoals_AccountId_IsExpired_QuestId",
                table: "UserGoals");

            migrationBuilder.DropIndex(
                name: "IX_UserGoals_AccountId_IsAchieved_IsExpired",
                table: "UserGoals");

            migrationBuilder.AddColumn<int>(
                name: "UserProfileId",
                table: "UserGoals",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE UG
                SET UG.UserProfileId = UP.Id
                FROM UserGoals UG
                JOIN UserProfiles UP ON UG.AccountId = UP.AccountId");

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "UserGoals",
                type: "int",
                nullable: false);

            migrationBuilder.CreateIndex(
                table: "UserGoals",
                name: "IX_UserGoals_UserProfileId_IsExpired_QuestId",
                columns: new[] { "UserProfileId", "IsExpired", "QuestId" });

            migrationBuilder.CreateIndex(
                table: "UserGoals",
                name: "IX_UserGoals_UserProfileId_IsAchieved_IsExpired",
                columns: new[] { "UserProfileId", "IsAchieved", "IsExpired" });

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "UserGoals");

            migrationBuilder.DropIndex(
                table: "Quests",
                name: "IX_Quests_AccountId_QuestType");

            migrationBuilder.DropIndex(
                table: "Quests",
                name: "IX_Quests_AccountId");

            migrationBuilder.AddColumn<int>(
                table: "Quests",
                name: "UserProfileId",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE Q
                SET Q.UserProfileId = UP.Id
                FROM Quests Q
                JOIN UserProfiles UP ON Q.AccountId = UP.AccountId");

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "Quests",
                type: "int",
                nullable: false);

            migrationBuilder.CreateIndex(
                table: "Quests",
                name: "IX_Quests_UserProfileId_QuestType",
                columns: new[] { "UserProfileId", "QuestType" });

            migrationBuilder.CreateIndex(
                table: "Quests",
                name: "IX_Quests_UserProfileId",
                column: "UserProfileId");

            migrationBuilder.DropColumn(
                table: "Quests",
                name: "AccountId");

            migrationBuilder.DropIndex(
                name: "IX_Quest_Labels_AccountId",
                table: "Quest_Labels");

            migrationBuilder.AddColumn<int>(
                name: "UserProfileId",
                table: "Quest_Labels",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE QL
                SET QL.UserProfileId = UP.Id
                FROM Quest_Labels QL
                JOIN UserProfiles UP ON QL.AccountId = UP.AccountId");

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "Quest_Labels",
                type: "int",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Quest_Labels_UserProfileId",
                table: "Quest_Labels",
                column: "UserProfileId");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Quest_Labels");

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "UserProfiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE UP
                SET UP.TimeZone = A.TimeZone
                FROM UserProfiles UP
                JOIN Accounts A ON A.Id = UP.AccountId");

            migrationBuilder.AlterColumn<string>(
                name: "TimeZone",
                table: "UserProfiles",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Etc/UTC");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Accounts");

            migrationBuilder.AddForeignKey(
                name: "FK_Quest_Labels_UserProfiles_UserProfileId",
                table: "Quest_Labels",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Quests_UserProfiles_UserProfileId",
                table: "Quests",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGoals_UserProfiles_UserProfileId",
                table: "UserGoals",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quest_Labels_UserProfiles_UserProfileId",
                table: "Quest_Labels");

            migrationBuilder.DropForeignKey(
                name: "FK_Quests_UserProfiles_UserProfileId",
                table: "Quests");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGoals_UserProfiles_UserProfileId",
                table: "UserGoals");

            // --- UserGoals ---
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "UserGoals",
                type: "int",
                nullable: false);

            migrationBuilder.Sql(
                @"UPDATE UG
                  SET UG.AccountId = UP.AccountId
                  FROM UserGoals UG
                  JOIN UserProfiles UP ON UG.UserProfileId = UP.Id");

            migrationBuilder.DropIndex(
                name: "IX_UserGoals_UserProfileId_IsExpired_QuestId",
                table: "UserGoals");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_AccountId_IsExpired_QuestId",
                table: "UserGoals",
                columns: new[] { "AccountId", "IsExpired", "QuestId" });

            migrationBuilder.DropIndex(
                name: "IX_UserGoals_UserProfileId_IsAchieved_IsExpired",
                table: "UserGoals");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_AccountId_IsAchieved_IsExpired",
                table: "UserGoals",
                columns: new[] { "AccountId", "IsAchieved", "IsExpired" });

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "UserGoals");

            // --- Quests ---
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Quests",
                type: "int",
                nullable: false);

            migrationBuilder.Sql(
                @"UPDATE Q
                  SET Q.AccountId = UP.AccountId
                  FROM Quests Q
                  JOIN UserProfiles UP ON Q.UserProfileId = UP.Id");

            migrationBuilder.DropIndex(
                name: "IX_Quests_UserProfileId_QuestType",
                table: "Quests");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_AccountId_QuestType",
                table: "Quests",
                columns: new[] { "AccountId", "QuestType" });

            migrationBuilder.DropIndex(
                name: "IX_Quests_UserProfileId",
                table: "Quests");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_AccountId",
                table: "Quests",
                column: "AccountId");

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "Quests");

            // --- Quest_Labels ---
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "Quest_Labels",
                type: "int",
                nullable: false);

            migrationBuilder.Sql(
                @"UPDATE QL
                  SET QL.AccountId = UP.AccountId
                  FROM Quest_Labels QL
                  JOIN UserProfiles UP ON QL.UserProfileId = UP.Id");

            migrationBuilder.DropIndex(
                name: "IX_Quest_Labels_UserProfileId",
                table: "Quest_Labels");

            migrationBuilder.CreateIndex(
                name: "IX_Quest_Labels_AccountId",
                table: "Quest_Labels",
                column: "AccountId");

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "Quest_Labels");

            // --- UserProfiles TimeZone revert ---
            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Accounts",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Etc/UTC");

            migrationBuilder.Sql(
                @"UPDATE A
                  SET A.TimeZone = UP.TimeZone
                  FROM Accounts A
                  JOIN UserProfiles UP ON A.Id = UP.AccountId");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "UserProfiles");

            // --- Recreate original FKs ---
            migrationBuilder.AddForeignKey(
                name: "FK_Quest_Labels_Accounts_AccountId",
                table: "Quest_Labels",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Quests_Accounts_AccountId",
                table: "Quests",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGoals_Accounts_AccountId",
                table: "UserGoals",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

    }
}
