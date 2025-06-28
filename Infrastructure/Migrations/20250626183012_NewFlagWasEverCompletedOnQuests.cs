using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewFlagWasEverCompletedOnQuests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompletedExistingQuests",
                table: "UserProfiles",
                newName: "EverCompletedExistingQuests");

            migrationBuilder.AddColumn<int>(
                name: "CurrentlyCompletedExistingQuests",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "WasEverCompleted",
                table: "Quests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentlyCompletedExistingQuests",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "WasEverCompleted",
                table: "Quests");

            migrationBuilder.RenameColumn(
                name: "EverCompletedExistingQuests",
                table: "UserProfiles",
                newName: "CompletedExistingQuests");
        }
    }
}
