using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrationAWS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Login = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    HashPassword = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TimeZone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Etc/UTC"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quest_Labels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    TextColor = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quest_Labels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quest_Labels_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    QuestType = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextResetAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quests_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bio = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TotalXp = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CompletedQuests = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    TotalQuests = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyQuest_Days",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    StartDay = table.Column<int>(type: "int", nullable: false),
                    EndDay = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyQuest_Days", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyQuest_Days_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quest_QuestLabel",
                columns: table => new
                {
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    QuestLabelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quest_QuestLabel", x => new { x.QuestId, x.QuestLabelId });
                    table.ForeignKey(
                        name: "FK_Quest_QuestLabel_Quest_Labels_QuestLabelId",
                        column: x => x.QuestLabelId,
                        principalTable: "Quest_Labels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Quest_QuestLabel_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonalQuest_Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    Season = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonalQuest_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonalQuest_Seasons_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeeklyQuest_Days",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    Weekday = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyQuest_Days", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeeklyQuest_Days_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfile_Badges",
                columns: table => new
                {
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    BadgeId = table.Column<int>(type: "int", nullable: false),
                    EarnedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfile_Badges", x => new { x.UserProfileId, x.BadgeId });
                    table.ForeignKey(
                        name: "FK_UserProfile_Badges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserProfile_Badges_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Badges_Type",
                table: "Badges",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyQuest_Days_QuestId",
                table: "MonthlyQuest_Days",
                column: "QuestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quest_Labels_AccountId",
                table: "Quest_Labels",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Quest_Labels_Value",
                table: "Quest_Labels",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_Quest_QuestLabel_QuestLabelId",
                table: "Quest_QuestLabel",
                column: "QuestLabelId");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_AccountId",
                table: "Quests",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_AccountId_QuestType",
                table: "Quests",
                columns: new[] { "AccountId", "QuestType" });

            migrationBuilder.CreateIndex(
                name: "IX_Quests_QuestType",
                table: "Quests",
                column: "QuestType");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalQuest_Seasons_QuestId",
                table: "SeasonalQuest_Seasons",
                column: "QuestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfile_Badges_BadgeId",
                table: "UserProfile_Badges",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfile_Badges_UserProfileId_BadgeId",
                table: "UserProfile_Badges",
                columns: new[] { "UserProfileId", "BadgeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_AccountId",
                table: "UserProfiles",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Nickname",
                table: "UserProfiles",
                column: "Nickname",
                unique: true,
                filter: "[Nickname] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyQuest_Days_QuestId",
                table: "WeeklyQuest_Days",
                column: "QuestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonthlyQuest_Days");

            migrationBuilder.DropTable(
                name: "Quest_QuestLabel");

            migrationBuilder.DropTable(
                name: "SeasonalQuest_Seasons");

            migrationBuilder.DropTable(
                name: "UserProfile_Badges");

            migrationBuilder.DropTable(
                name: "WeeklyQuest_Days");

            migrationBuilder.DropTable(
                name: "Quest_Labels");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
