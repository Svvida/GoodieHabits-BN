using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUnnecessaryLevelingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentExperience",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "UserLevel",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "TotalExperience",
                table: "UserProfiles",
                newName: "TotalXp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalXp",
                table: "UserProfiles",
                newName: "TotalExperience");

            migrationBuilder.AddColumn<int>(
                name: "CurrentExperience",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserLevel",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }
    }
}
