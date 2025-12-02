using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExtendedAvatarFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Avatar",
                table: "UserProfiles",
                newName: "UploadedAvatarUrl");

            migrationBuilder.AddColumn<string>(
                name: "CurrentAvatarUrl",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql(
                @"UPDATE UserProfiles
                SET CurrentAvatarUrl = UploadedAvatarUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentAvatarUrl",
                table: "UserProfiles");

            migrationBuilder.RenameColumn(
                name: "UploadedAvatarUrl",
                table: "UserProfiles",
                newName: "Avatar");
        }
    }
}
