using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRelationshipAndAddMoreStatsToProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserGoals_QuestId",
                table: "UserGoals");

            migrationBuilder.AddColumn<int>(
                name: "AbandonedGoals",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompletedGoals",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExpiredGoals",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_QuestId",
                table: "UserGoals",
                column: "QuestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserGoals_QuestId",
                table: "UserGoals");

            migrationBuilder.DropColumn(
                name: "AbandonedGoals",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CompletedGoals",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "ExpiredGoals",
                table: "UserProfiles");

            migrationBuilder.CreateIndex(
                name: "IX_UserGoals_QuestId",
                table: "UserGoals",
                column: "QuestId",
                unique: true);
        }
    }
}
