using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// Step 2 of moving quest occurrences to local calendar periods.
    /// <para>
    /// ⚠️ Apply this only after <c>BackfillQuestOccurrencePeriodsTask</c> has run to completion — it is
    /// what populates <c>PeriodStart</c>/<c>PeriodEnd</c> and removes the duplicate periods that
    /// timezone drift produced under the old instant-based scheme. The guard below fails loudly rather
    /// than letting the <c>NOT NULL</c> alter corrupt rows with a default date.
    /// </para>
    /// </summary>
    public partial class QuestCalendarPeriods_Step2_Finalize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM QuestOccurrences WHERE PeriodStart IS NULL OR PeriodEnd IS NULL)
    THROW 50000, 'QuestOccurrences still contain unpopulated periods. Run the application once so BackfillQuestOccurrencePeriodsTask can complete, then re-apply this migration.', 1;

IF EXISTS (SELECT QuestId, PeriodStart FROM QuestOccurrences GROUP BY QuestId, PeriodStart HAVING COUNT(*) > 1)
    THROW 50000, 'QuestOccurrences still contain duplicate (QuestId, PeriodStart) rows. Run the application once so BackfillQuestOccurrencePeriodsTask can de-duplicate them, then re-apply this migration.', 1;
");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PeriodStart",
                table: "QuestOccurrences",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PeriodEnd",
                table: "QuestOccurrences",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.DropIndex(
                name: "IX_QuestOccurrences_OccurrenceStart",
                table: "QuestOccurrences");

            migrationBuilder.DropIndex(
                name: "IX_QuestOccurrences_QuestId_OccurrenceStart_OccurrenceEnd",
                table: "QuestOccurrences");

            migrationBuilder.DropColumn(
                name: "OccurrenceStart",
                table: "QuestOccurrences");

            migrationBuilder.DropColumn(
                name: "OccurrenceEnd",
                table: "QuestOccurrences");

            // The real guarantee this whole refactor buys: one occurrence per quest per period,
            // enforced by the database and immune to the user's timezone changing.
            migrationBuilder.CreateIndex(
                name: "IX_QuestOccurrences_QuestId_PeriodStart",
                table: "QuestOccurrences",
                columns: new[] { "QuestId", "PeriodStart" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestOccurrences_QuestId_PeriodStart",
                table: "QuestOccurrences");

            migrationBuilder.AddColumn<DateTime>(
                name: "OccurrenceStart",
                table: "QuestOccurrences",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "OccurrenceEnd",
                table: "QuestOccurrences",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Best-effort reconstruction: midnight-to-midnight UTC, without the user's timezone.
            migrationBuilder.Sql(@"
UPDATE QuestOccurrences
SET OccurrenceStart = CAST(PeriodStart AS datetime2),
    OccurrenceEnd   = DATEADD(day, 1, CAST(PeriodEnd AS datetime2));
");

            migrationBuilder.CreateIndex(
                name: "IX_QuestOccurrences_OccurrenceStart",
                table: "QuestOccurrences",
                column: "OccurrenceStart");

            migrationBuilder.CreateIndex(
                name: "IX_QuestOccurrences_QuestId_OccurrenceStart_OccurrenceEnd",
                table: "QuestOccurrences",
                columns: new[] { "QuestId", "OccurrenceStart", "OccurrenceEnd" },
                unique: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PeriodEnd",
                table: "QuestOccurrences",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "PeriodStart",
                table: "QuestOccurrences",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
