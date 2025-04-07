using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestoreIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QuestType",
                table: "Quests",
                type: "nvarchar(450)",
                nullable: false,
                collation: "Latin1_General_100_CI_AS_SC_UTF8",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldCollation: "Latin1_General_100_CI_AS_SC_UTF8");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Nickname",
                table: "UserProfiles",
                column: "Nickname",
                unique: true,
                filter: "[Nickname] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_AccountId_QuestType",
                table: "Quests",
                columns: new[] { "AccountId", "QuestType" });

            migrationBuilder.CreateIndex(
                name: "IX_Quests_QuestType",
                table: "Quests",
                column: "QuestType");

            migrationBuilder.CreateIndex(
                name: "IX_Quest_Labels_Value",
                table: "Quest_Labels",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_Badges_Type",
                table: "Badges",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Login",
                table: "Accounts",
                column: "Login",
                unique: true,
                filter: "[Login] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_Nickname",
                table: "UserProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Quests_AccountId_QuestType",
                table: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_Quests_QuestType",
                table: "Quests");

            migrationBuilder.DropIndex(
                name: "IX_Quest_Labels_Value",
                table: "Quest_Labels");

            migrationBuilder.DropIndex(
                name: "IX_Badges_Type",
                table: "Badges");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Email",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Login",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "QuestType",
                table: "Quests",
                type: "nvarchar(max)",
                nullable: false,
                collation: "Latin1_General_100_CI_AS_SC_UTF8",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldCollation: "Latin1_General_100_CI_AS_SC_UTF8");
        }
    }
}
