using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quest_Labels_Accounts_AccountId",
                table: "Quest_Labels");

            migrationBuilder.DropTable(
                name: "Daily_Quests");

            migrationBuilder.DropTable(
                name: "Monthly_Quests");

            migrationBuilder.DropTable(
                name: "One_Time_Quests");

            migrationBuilder.DropTable(
                name: "QuestMetadata_QuestLabel");

            migrationBuilder.DropTable(
                name: "Seasonal_Quests");

            migrationBuilder.DropTable(
                name: "Weekly_Quests");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Username",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Accounts",
                newName: "Nickname");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Quests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Quests",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "Quests",
                type: "NVARCHAR(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Quests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Quests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCompletedAt",
                table: "Quests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextResetAt",
                table: "Quests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Quests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Quests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Quests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Quests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Accounts",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Etc/UTC");

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
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    QuestsCompleted = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Nickname",
                table: "Accounts",
                column: "Nickname",
                unique: true,
                filter: "[Nickname] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyQuest_Days_QuestId",
                table: "MonthlyQuest_Days",
                column: "QuestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quest_QuestLabel_QuestLabelId",
                table: "Quest_QuestLabel",
                column: "QuestLabelId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalQuest_Seasons_QuestId",
                table: "SeasonalQuest_Seasons",
                column: "QuestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_AccountId",
                table: "UserProfiles",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyQuest_Days_QuestId",
                table: "WeeklyQuest_Days",
                column: "QuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quest_Labels_Accounts_AccountId",
                table: "Quest_Labels",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quest_Labels_Accounts_AccountId",
                table: "Quest_Labels");

            migrationBuilder.DropTable(
                name: "MonthlyQuest_Days");

            migrationBuilder.DropTable(
                name: "Quest_QuestLabel");

            migrationBuilder.DropTable(
                name: "SeasonalQuest_Seasons");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "WeeklyQuest_Days");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_Nickname",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "LastCompletedAt",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "NextResetAt",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Quests");

            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Accounts");

            migrationBuilder.RenameColumn(
                name: "Nickname",
                table: "Accounts",
                newName: "Username");

            migrationBuilder.CreateTable(
                name: "Daily_Quests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    LastCompleted = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Daily_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Daily_Quests_Quests_Id",
                        column: x => x.Id,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Monthly_Quests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDay = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartDay = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monthly_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Monthly_Quests_Quests_Id",
                        column: x => x.Id,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "One_Time_Quests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_One_Time_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_One_Time_Quests_Quests_Id",
                        column: x => x.Id,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestMetadata_QuestLabel",
                columns: table => new
                {
                    QuestMetadataId = table.Column<int>(type: "int", nullable: false),
                    QuestLabelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestMetadata_QuestLabel", x => new { x.QuestMetadataId, x.QuestLabelId });
                    table.ForeignKey(
                        name: "FK_QuestMetadata_QuestLabel_Quest_Labels_QuestLabelId",
                        column: x => x.QuestLabelId,
                        principalTable: "Quest_Labels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestMetadata_QuestLabel_Quests_QuestMetadataId",
                        column: x => x.QuestMetadataId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seasonal_Quests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    Season = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasonal_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasonal_Quests_Quests_Id",
                        column: x => x.Id,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Weekly_Quests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    Emoji = table.Column<string>(type: "NVARCHAR(10)", maxLength: 10, nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WeekdaysSerialized = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weekly_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Weekly_Quests_Quests_Id",
                        column: x => x.Id,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Username",
                table: "Accounts",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestMetadata_QuestLabel_QuestLabelId",
                table: "QuestMetadata_QuestLabel",
                column: "QuestLabelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quest_Labels_Accounts_AccountId",
                table: "Quest_Labels",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
