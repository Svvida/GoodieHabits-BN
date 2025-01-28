using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangePriorityName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PriorityLevel",
                table: "Repeatable_Quests",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "PriorityLevel",
                table: "One_Time_Quests",
                newName: "Priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "Repeatable_Quests",
                newName: "PriorityLevel");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "One_Time_Quests",
                newName: "PriorityLevel");
        }
    }
}
