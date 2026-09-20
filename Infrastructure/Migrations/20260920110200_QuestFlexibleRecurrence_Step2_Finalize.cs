using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// The destructive half of the flexible-recurrence change: drops the <c>QuestType</c> discriminator, the
    /// derived <c>IsCompleted</c> / <c>NextResetAt</c> columns, and the three satellite tables the old model
    /// needed to describe a schedule.
    /// <para>
    /// Split from step 1 so the backfill can read those columns before they disappear, following the
    /// <c>QuestCalendarPeriods</c> precedent. It hard-fails if the backfill has not run, because dropping
    /// <c>QuestType</c> with schedules still unset would leave every quest unschedulable and unrecoverable
    /// without a restore.
    /// </para>
    /// <para>
    /// ⚠️ Its id was moved from <c>20260912125101</c> to <c>20260920110200</c> so that it sorts after
    /// <c>Step1b_SweepBackfill</c>. Migrations apply in id order, and this one drops the very columns the
    /// sweep reads — with the original id, a plain <c>database update</c> would have run them the wrong way
    /// round. Safe to renumber because it had not been applied anywhere.
    /// </para>
    /// </summary>
    /// <inheritdoc />
    public partial class QuestFlexibleRecurrence_Step2_Finalize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sweep up anything the previously deployed API wrote between the two steps: it still creates
            // quests the old way, and those rows take the column default rather than a real schedule. The
            // backfill is scoped to unmigrated rows, so this cannot disturb what the new code has written.
            QuestRecurrenceBackfill.Run(migrationBuilder);

            GuardBackfillHasRun(migrationBuilder);

            migrationBuilder.DropTable(name: "MonthlyQuest_Days");
            migrationBuilder.DropTable(name: "SeasonalQuest_Seasons");
            migrationBuilder.DropTable(name: "WeeklyQuest_Days");

            migrationBuilder.DropIndex(name: "IX_Quests_QuestType", table: "Quests");
            migrationBuilder.DropIndex(name: "IX_Quests_UserProfileId_QuestType", table: "Quests");

            migrationBuilder.DropColumn(name: "QuestType", table: "Quests");
            migrationBuilder.DropColumn(name: "IsCompleted", table: "Quests");
            migrationBuilder.DropColumn(name: "NextResetAt", table: "Quests");

            // Completion is now "progress reached the target", derived from the column beside it.
            migrationBuilder.DropColumn(name: "WasCompleted", table: "QuestOccurrences");
        }

        /// <summary>
        /// Refuses to run unless every quest came out of step 1 with a real schedule. An empty
        /// <c>Schedule_Unit</c> means the backfill was skipped or failed part-way.
        /// </summary>
        private static void GuardBackfillHasRun(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM Quests WHERE Schedule_Unit IS NULL OR Schedule_Unit = '')
                BEGIN
                    THROW 51000, 'QuestFlexibleRecurrence: quests still have no schedule. Run step 1 (and its backfill) before this migration.', 1;
                END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuestType",
                table: "Quests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Quests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextResetAt",
                table: "Quests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasCompleted",
                table: "QuestOccurrences",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(name: "IX_Quests_QuestType", table: "Quests", column: "QuestType");
            migrationBuilder.CreateIndex(
                name: "IX_Quests_UserProfileId_QuestType",
                table: "Quests",
                columns: new[] { "UserProfileId", "QuestType" });

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

            migrationBuilder.CreateIndex(name: "IX_MonthlyQuest_Days_QuestId", table: "MonthlyQuest_Days", column: "QuestId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_SeasonalQuest_Seasons_QuestId", table: "SeasonalQuest_Seasons", column: "QuestId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_WeeklyQuest_Days_QuestId", table: "WeeklyQuest_Days", column: "QuestId");
        }
    }
}
