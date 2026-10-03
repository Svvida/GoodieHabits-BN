using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <summary>
    /// Re-runs <see cref="QuestRecurrenceBackfill"/> immediately before the new API is deployed.
    /// <para>
    /// Step 1 shipped on 2026-09-12 and the <em>old</em> API kept serving traffic for eight days after it,
    /// writing rows the old way: completions recorded only as <c>WasCompleted</c>, with no progress and no
    /// entry in the completion log. Migrations run once, so step 1's copy of the backfill could not pick
    /// those up. This migration exists purely to give it a second run — that is why it has no schema
    /// changes at all.
    /// </para>
    /// <para>
    /// It also repairs a gap in step 1's own backfill: the completion log was written <em>before</em> the
    /// pass that materializes periods for one-off quests, and keyed on the legacy <c>WasCompleted</c> flag
    /// that those inserted rows never carry. Seventeen completed one-off quests ended up with no log entry,
    /// so they were missing from tap totals and from the weekday and hour-of-day breakdowns. The ordering
    /// and the predicate are fixed in the shared helper, so step 2 inherits the fix.
    /// </para>
    /// <para>
    /// Safe to run at any time: every statement is idempotent and scoped to rows the new model has not
    /// claimed, so it cannot disturb anything the new API has written.
    /// </para>
    /// </summary>
    /// <inheritdoc />
    public partial class QuestFlexibleRecurrence_Step1b_SweepBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            QuestRecurrenceBackfill.Run(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to undo: this migration only fills in values the new model needs, and step 1's Down
            // drops the columns holding them. Reverting the data would mean re-deriving the old
            // representation from the new one, which is exactly the direction that loses information.
        }
    }
}
