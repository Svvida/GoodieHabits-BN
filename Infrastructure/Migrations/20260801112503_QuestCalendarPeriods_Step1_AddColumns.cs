using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// Step 1 of moving quest occurrences from UTC instants to local calendar periods.
    /// <para>
    /// Additive only: the new period columns land nullable and the old <c>OccurrenceStart</c>/
    /// <c>OccurrenceEnd</c> columns stay put, so applying this does not destroy anything.
    /// <c>BackfillQuestOccurrencePeriodsTask</c> then populates and de-duplicates the new columns
    /// (reading the old ones via raw SQL), and <c>QuestCalendarPeriods_Step2_Finalize</c> enforces the
    /// constraints and drops the old columns.
    /// </para>
    /// <para>
    /// ⚠️ The <c>Quests.StartDate</c>/<c>EndDate</c> conversion below is a plain SQL
    /// <c>datetime2 -&gt; date</c> truncation, keeping the UTC date component. Occurrence periods get a
    /// proper timezone-aware projection in the backfill because their old values were generated at local
    /// midnight by construction; quest start/end dates carry no such guarantee, so there is no
    /// convention to project them through.
    /// </para>
    /// </summary>
    public partial class QuestCalendarPeriods_Step1_AddColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A quest's active range is a calendar fact, not an instant.
            migrationBuilder.AlterColumn<DateOnly>(
                name: "StartDate",
                table: "Quests",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "EndDate",
                table: "Quests",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            // Nullable for now — the backfill task fills these in, step 2 makes them required.
            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodStart",
                table: "QuestOccurrences",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "PeriodEnd",
                table: "QuestOccurrences",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBackfilled",
                table: "QuestOccurrences",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBackfilled",
                table: "QuestOccurrences");

            migrationBuilder.DropColumn(
                name: "PeriodEnd",
                table: "QuestOccurrences");

            migrationBuilder.DropColumn(
                name: "PeriodStart",
                table: "QuestOccurrences");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "Quests",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "Quests",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
