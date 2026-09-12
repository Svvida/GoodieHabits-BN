using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuestFlexibleRecurrence_Step1_AddColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "MaintainedThrough",
                table: "UserProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeekStartsOn",
                table: "UserProfiles",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Monday");

            migrationBuilder.AddColumn<int>(
                name: "PartialCount",
                table: "QuestStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalCompletions",
                table: "QuestStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "Quests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Schedule_Interval",
                table: "Quests",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Schedule_MonthWindowEndDay",
                table: "Quests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Schedule_MonthWindowStartDay",
                table: "Quests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Schedule_Unit",
                table: "Quests",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Schedule_Weekdays",
                table: "Quests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Schedule_YearWindowEnd",
                table: "Quests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Schedule_YearWindowStart",
                table: "Quests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Target_Amount",
                table: "Quests",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<int>(
                name: "Target_MaxCompletionsPerDay",
                table: "Quests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Target_Mode",
                table: "Quests",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "AtLeast");

            migrationBuilder.AddColumn<string>(
                name: "Target_Unit",
                table: "Quests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CoinsAwarded",
                table: "QuestOccurrences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Progress",
                table: "QuestOccurrences",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "RewardGrantedAt",
                table: "QuestOccurrences",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "QuestOccurrences",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "SkipReason",
                table: "QuestOccurrences",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SkippedAt",
                table: "QuestOccurrences",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TargetAmount",
                table: "QuestOccurrences",
                type: "decimal(9,2)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<int>(
                name: "XpAwarded",
                table: "QuestOccurrences",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QuestCompletions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestId = table.Column<int>(type: "int", nullable: false),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    OccurrenceId = table.Column<int>(type: "int", nullable: true),
                    CompletedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocalTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(9,2)", nullable: false, defaultValue: 1m),
                    IsBackfilled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestCompletions_QuestOccurrences_OccurrenceId",
                        column: x => x.OccurrenceId,
                        principalTable: "QuestOccurrences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestCompletions_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestCompletions_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Quests_Schedule_Unit",
                table: "Quests",
                column: "Schedule_Unit");

            migrationBuilder.CreateIndex(
                name: "IX_QuestCompletions_OccurrenceId",
                table: "QuestCompletions",
                column: "OccurrenceId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestCompletions_QuestId_ClientRequestId",
                table: "QuestCompletions",
                columns: new[] { "QuestId", "ClientRequestId" },
                unique: true,
                filter: "[ClientRequestId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_QuestCompletions_QuestId_CompletedOn",
                table: "QuestCompletions",
                columns: new[] { "QuestId", "CompletedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestCompletions_UserProfileId_CompletedOn",
                table: "QuestCompletions",
                columns: new[] { "UserProfileId", "CompletedOn" });

            KeepLegacyColumnsInsertable(migrationBuilder);

            QuestRecurrenceBackfill.Run(migrationBuilder);
        }

        /// <inheritdoc />

        /// <summary>
        /// The legacy columns survive until step 2, which is what makes this migration reversible — but the
        /// new code no longer maps them, so an INSERT omits them entirely. <c>IsCompleted</c> and
        /// <c>WasCompleted</c> already carry default constraints; <c>QuestType</c> is NOT NULL with none, so
        /// creating a quest between the two steps would fail outright.
        /// <para>
        /// The default is a valid enum name rather than an empty string so that an app rolled back to the
        /// previous version can still parse rows created during the window.
        /// </para>
        /// </summary>
        private static void KeepLegacyColumnsInsertable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.default_constraints d
                    INNER JOIN sys.columns c ON d.parent_object_id = c.object_id AND d.parent_column_id = c.column_id
                    WHERE d.parent_object_id = OBJECT_ID(N'[Quests]') AND c.name = N'QuestType')
                BEGIN
                    ALTER TABLE [Quests] ADD CONSTRAINT [DF_Quests_QuestType_Legacy] DEFAULT N'Daily' FOR [QuestType];
                END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "QuestCompletions");

            foreach (var column in new[]
            {
                "Schedule_Unit", "Schedule_Interval", "Schedule_Weekdays",
                "Schedule_MonthWindowStartDay", "Schedule_MonthWindowEndDay",
                "Schedule_YearWindowStart", "Schedule_YearWindowEnd",
                "Target_Amount", "Target_Unit", "Target_Mode", "Target_MaxCompletionsPerDay",
                "DurationMinutes"
            })
            {
                migrationBuilder.DropColumn(name: column, table: "Quests");
            }

            foreach (var column in new[]
            {
                "TargetAmount", "Progress", "RewardGrantedAt", "XpAwarded",
                "CoinsAwarded", "SkippedAt", "SkipReason", "RowVersion"
            })
            {
                migrationBuilder.DropColumn(name: column, table: "QuestOccurrences");
            }

            migrationBuilder.DropColumn(name: "PartialCount", table: "QuestStatistics");
            migrationBuilder.DropColumn(name: "TotalCompletions", table: "QuestStatistics");
            migrationBuilder.DropColumn(name: "WeekStartsOn", table: "UserProfiles");
            migrationBuilder.DropColumn(name: "MaintainedThrough", table: "UserProfiles");
        }
    }
}
