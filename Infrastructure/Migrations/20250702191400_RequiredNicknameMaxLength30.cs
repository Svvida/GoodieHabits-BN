using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RequiredNicknameMaxLength30 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_Nickname",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "Nickname",
                table: "UserProfiles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Nickname",
                table: "UserProfiles",
                column: "Nickname",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserProfiles_Nickname",
                table: "UserProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "Nickname",
                table: "UserProfiles",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Nickname",
                table: "UserProfiles",
                column: "Nickname",
                unique: true,
                filter: "[Nickname] IS NOT NULL");
        }
    }
}
