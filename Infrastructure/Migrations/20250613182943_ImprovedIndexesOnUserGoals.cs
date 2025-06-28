using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImprovedIndexesOnUserGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserGoals_AccountId",
                table: "UserGoals");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_AccountId_IsExpired_QuestId",
                table: "UserGoals",
                columns: new[] { "AccountId", "IsExpired", "QuestId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserGoals_AccountId_IsExpired_QuestId",
                table: "UserGoals");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_AccountId",
                table: "UserGoals",
                column: "AccountId");
        }
    }
}
