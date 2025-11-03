using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FriendsFeatureDatabaseSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FriendsCount",
                table: "UserProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FriendInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderUserProfileId = table.Column<int>(type: "int", nullable: false),
                    ReceiverUserProfileId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FriendInvitations_UserProfiles_ReceiverUserProfileId",
                        column: x => x.ReceiverUserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FriendInvitations_UserProfiles_SenderUserProfileId",
                        column: x => x.SenderUserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    UserProfileId1 = table.Column<int>(type: "int", nullable: false),
                    UserProfileId2 = table.Column<int>(type: "int", nullable: false),
                    BecameFriendsAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => new { x.UserProfileId1, x.UserProfileId2 });
                    table.ForeignKey(
                        name: "FK_Friendships_UserProfiles_UserProfileId1",
                        column: x => x.UserProfileId1,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Friendships_UserProfiles_UserProfileId2",
                        column: x => x.UserProfileId2,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserBlocks",
                columns: table => new
                {
                    BlockerUserProfileId = table.Column<int>(type: "int", nullable: false),
                    BlockedUserProfileId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBlocks", x => new { x.BlockerUserProfileId, x.BlockedUserProfileId });
                    table.ForeignKey(
                        name: "FK_UserBlocks_UserProfiles_BlockedUserProfileId",
                        column: x => x.BlockedUserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserBlocks_UserProfiles_BlockerUserProfileId",
                        column: x => x.BlockerUserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendInvitations_ReceiverUserProfileId",
                table: "FriendInvitations",
                column: "ReceiverUserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendInvitations_SenderUserProfileId_ReceiverUserProfileId_Status",
                table: "FriendInvitations",
                columns: new[] { "SenderUserProfileId", "ReceiverUserProfileId", "Status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserProfileId1_UserProfileId2",
                table: "Friendships",
                columns: new[] { "UserProfileId1", "UserProfileId2" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UserProfileId2",
                table: "Friendships",
                column: "UserProfileId2");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_BlockedUserProfileId",
                table: "UserBlocks",
                column: "BlockedUserProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBlocks_BlockerUserProfileId_BlockedUserProfileId",
                table: "UserBlocks",
                columns: new[] { "BlockerUserProfileId", "BlockedUserProfileId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendInvitations");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "UserBlocks");

            migrationBuilder.DropColumn(
                name: "FriendsCount",
                table: "UserProfiles");
        }
    }
}
