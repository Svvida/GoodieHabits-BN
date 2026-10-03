using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Migrations
{
    /// <summary>
    /// Translates legacy quest rows into the schedule/target/completion model.
    /// <para>
    /// Shared by both steps of the change, and <b>every statement is idempotent and scoped to unmigrated
    /// rows</b>. That is not tidiness — it is what makes a phased deploy safe. Between step 1 and the release
    /// of the new API, the previously deployed version is still running and still writing rows the old way:
    /// quests whose <c>Schedule_Unit</c> falls back to the column default, and occurrences whose completion
    /// lives only in <c>WasCompleted</c>. Step 2 runs this again to sweep those up before it drops anything.
    /// </para>
    /// <para>
    /// The scoping matters in the other direction too. A quest created by the <em>new</em> code takes the
    /// legacy <c>QuestType</c> column's default, so an unguarded
    /// <c>UPDATE ... WHERE QuestType = 'Daily'</c> would happily overwrite a correct Week schedule with a
    /// Day one. Hence every update also requires the schedule to be unset.
    /// </para>
    /// </summary>
    internal static class QuestRecurrenceBackfill
    {
        /// <summary>Rows the new model has not claimed yet — the only ones any of this may touch.</summary>
        private const string Unclassified = "(q.Schedule_Unit IS NULL OR q.Schedule_Unit = '')";

        public static void Run(MigrationBuilder migrationBuilder)
        {
            ClassifySchedules(migrationBuilder);
            BackfillPeriods(migrationBuilder);

            // Order matters: the non-repeatable pass INSERTS periods (carrying their own CompletedAt), so
            // the completion log has to be written after them or those rows never get one. The first run of
            // this backfill had these two the other way round and left 17 completed one-off quests with no
            // log entry — invisible in totals and in the weekday/hour breakdowns.
            BackfillNonRepeatablePeriods(migrationBuilder);
            BackfillCompletionLog(migrationBuilder);
        }

        /// <summary>
        /// Every legacy <c>QuestType</c> becomes a schedule. Plain SQL rather than the C# task the previous
        /// quest migration needed, because none of this touches a timezone — the satellite tables already
        /// held calendar facts.
        /// </summary>
        private static void ClassifySchedules(MigrationBuilder migrationBuilder)
        {
            // OneTime -> no recurrence: one period spanning the quest's active range.
            migrationBuilder.Sql($@"
                UPDATE q SET q.Schedule_Unit = 'None'
                FROM Quests q
                WHERE q.QuestType = 'OneTime' AND {Unclassified};");

            migrationBuilder.Sql($@"
                UPDATE q SET q.Schedule_Unit = 'Day'
                FROM Quests q
                WHERE q.QuestType = 'Daily' AND {Unclassified};");

            // Weekly -> a Day schedule filtered to the weekdays it ran on, which is what "Weekly" always
            // actually meant. Weekday was stored as the DayOfWeek ordinal and WeekdayFlags is 1 << ordinal,
            // so the whole set collapses into a single integer.
            migrationBuilder.Sql($@"
                UPDATE q
                SET q.Schedule_Unit = 'Day',
                    q.Schedule_Weekdays = wd.Flags
                FROM Quests q
                INNER JOIN (
                    SELECT QuestId, SUM(DISTINCT CAST(POWER(2, Weekday) AS int)) AS Flags
                    FROM WeeklyQuest_Days
                    GROUP BY QuestId
                ) wd ON wd.QuestId = q.Id
                WHERE q.QuestType = 'Weekly' AND {Unclassified};");

            // A Weekly quest with no weekday rows should not exist; if one does, leave it due every day
            // rather than silently dropping it out of the schedule altogether.
            migrationBuilder.Sql($@"
                UPDATE q SET q.Schedule_Unit = 'Day'
                FROM Quests q
                WHERE q.QuestType = 'Weekly' AND {Unclassified};");

            migrationBuilder.Sql($@"
                UPDATE q
                SET q.Schedule_Unit = 'Month',
                    q.Schedule_MonthWindowStartDay = md.StartDay,
                    q.Schedule_MonthWindowEndDay = md.EndDay
                FROM Quests q
                INNER JOIN MonthlyQuest_Days md ON md.QuestId = q.Id
                WHERE q.QuestType = 'Monthly' AND {Unclassified};");

            migrationBuilder.Sql($@"
                UPDATE q SET q.Schedule_Unit = 'Month'
                FROM Quests q
                WHERE q.QuestType = 'Monthly' AND {Unclassified};");

            // Seasonal -> a yearly window. Winter wraps past new year, which the calculator handles by
            // treating an end before the start as running into the following year. Boundaries match the
            // SeasonHelper they replace. Season was stored as the enum ordinal: 0 Winter .. 3 Autumn.
            migrationBuilder.Sql($@"
                UPDATE q
                SET q.Schedule_Unit = 'Year',
                    q.Schedule_YearWindowStart = CASE s.Season WHEN 0 THEN 1221 WHEN 1 THEN 321 WHEN 2 THEN 621 ELSE 923 END,
                    q.Schedule_YearWindowEnd   = CASE s.Season WHEN 0 THEN 320  WHEN 1 THEN 620 WHEN 2 THEN 922 ELSE 1220 END
                FROM Quests q
                INNER JOIN SeasonalQuest_Seasons s ON s.QuestId = q.Id
                WHERE q.QuestType = 'Seasonal' AND {Unclassified};");

            // Anything the cases above did not reach (a type with no satellite row) becomes a one-off, rather
            // than being left with an empty unit that step 2 would refuse to proceed past.
            migrationBuilder.Sql($@"
                UPDATE q SET q.Schedule_Unit = 'None'
                FROM Quests q
                WHERE {Unclassified};");

            // Every legacy quest is a plain "do it once per period" habit; richer targets are new.
            migrationBuilder.Sql(@"
                UPDATE Quests
                SET Target_Amount = 1, Target_Mode = 'AtLeast'
                WHERE Target_Amount IS NULL OR Target_Amount = 0 OR Target_Mode IS NULL OR Target_Mode = '';");
        }

        /// <summary>
        /// Legacy periods asked for exactly one completion, and a completed one has already been paid for —
        /// stamping <c>RewardGrantedAt</c> is what stops the new per-period payout rewarding history twice.
        /// <para>
        /// Scoped to <c>WasCompleted = 1 AND Progress = 0</c>: precisely "completed the old way, not yet
        /// migrated". Without that, re-running this would wipe progress recorded by the new code.
        /// </para>
        /// </summary>
        private static void BackfillPeriods(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE QuestOccurrences
                SET TargetAmount = CASE WHEN TargetAmount IS NULL OR TargetAmount = 0 THEN 1 ELSE TargetAmount END,
                    Progress = 1,
                    RewardGrantedAt = COALESCE(RewardGrantedAt, CompletedAt)
                WHERE WasCompleted = 1 AND Progress = 0;");

            migrationBuilder.Sql(@"
                UPDATE QuestOccurrences
                SET TargetAmount = 1
                WHERE TargetAmount IS NULL OR TargetAmount = 0;");
        }

        /// <summary>
        /// One completion row per completed period — whether it was completed the old way, by an earlier
        /// step of this backfill, or by the new API — so streaks, totals and the weekday breakdown keep
        /// working. <c>LocalTime</c> stays null: recovering the user's wall-clock hour would need the
        /// timezone they held at the time, and guessing it is worse than reporting nothing — the hour-of-day
        /// breakdown skips rows without it by design.
        /// </summary>
        private static void BackfillCompletionLog(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO QuestCompletions
                    (QuestId, UserProfileId, OccurrenceId, CompletedOn, CompletedAt, LocalTime,
                     Amount, IsBackfilled, Source, ClientRequestId, Note, CreatedAt)
                SELECT
                    qo.QuestId,
                    q.UserProfileId,
                    qo.Id,
                    -- Single-day periods are exact. For a multi-day period the completion instant's UTC date
                    -- can be a day out, so it is clamped into the period it demonstrably belongs to.
                    CASE
                        WHEN qo.PeriodStart = qo.PeriodEnd THEN qo.PeriodStart
                        WHEN CAST(qo.CompletedAt AS date) < qo.PeriodStart THEN qo.PeriodStart
                        WHEN CAST(qo.CompletedAt AS date) > qo.PeriodEnd THEN qo.PeriodEnd
                        ELSE CAST(qo.CompletedAt AS date)
                    END,
                    qo.CompletedAt,
                    NULL,
                    1,
                    qo.IsBackfilled,
                    'App',
                    NULL,
                    NULL,
                    qo.CompletedAt
                FROM QuestOccurrences qo
                INNER JOIN Quests q ON q.Id = qo.QuestId
                -- Keyed on CompletedAt, not on the legacy WasCompleted flag: periods this migration
                -- inserts for one-off quests never carry that flag, and a period completed through the new
                -- API does not set it either. NOT EXISTS keeps it idempotent and leaves new-API taps alone,
                -- since those already wrote their own log row.
                WHERE qo.CompletedAt IS NOT NULL
                  AND NOT EXISTS (SELECT 1 FROM QuestCompletions qc WHERE qc.OccurrenceId = qo.Id);");
        }

        /// <summary>
        /// One-time and seasonal quests never had periods, because the old model did not consider them
        /// repeatable — yet their completed flag was real state. Each gets the single period it now owns so
        /// that state survives.
        /// <para>
        /// Only the season covering the migration date is materialized for a seasonal quest. Earlier seasons
        /// are deliberately not invented: fabricating periods the user was never asked about is exactly the
        /// damage the previous quest migration had to repair.
        /// </para>
        /// </summary>
        private static void BackfillNonRepeatablePeriods(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO QuestOccurrences
                    (QuestId, PeriodStart, PeriodEnd, TargetAmount, Progress, CompletedAt, IsBackfilled,
                     RewardGrantedAt, XpAwarded, CoinsAwarded)
                SELECT
                    q.Id,
                    COALESCE(q.StartDate, CAST(q.CreatedAt AS date)),
                    COALESCE(q.EndDate, '9999-12-31'),
                    1,
                    CASE WHEN q.IsCompleted = 1 THEN 1 ELSE 0 END,
                    CASE WHEN q.IsCompleted = 1 THEN COALESCE(q.LastCompletedAt, q.CreatedAt) ELSE NULL END,
                    0,
                    CASE WHEN q.IsCompleted = 1 THEN COALESCE(q.LastCompletedAt, q.CreatedAt) ELSE NULL END,
                    0,
                    0
                FROM Quests q
                WHERE q.QuestType = 'OneTime'
                  AND q.Schedule_Unit = 'None'
                  AND NOT EXISTS (SELECT 1 FROM QuestOccurrences o WHERE o.QuestId = q.Id);");

            migrationBuilder.Sql(@"
                WITH seasons AS (
                    SELECT
                        q.Id AS QuestId,
                        q.IsCompleted,
                        q.LastCompletedAt,
                        q.CreatedAt,
                        DATEFROMPARTS(
                            CASE WHEN s.Season = 0 AND MONTH(GETUTCDATE()) <= 3 THEN YEAR(GETUTCDATE()) - 1 ELSE YEAR(GETUTCDATE()) END,
                            CASE s.Season WHEN 0 THEN 12 WHEN 1 THEN 3 WHEN 2 THEN 6 ELSE 9 END,
                            CASE s.Season WHEN 0 THEN 21 WHEN 1 THEN 21 WHEN 2 THEN 21 ELSE 23 END) AS PeriodStart,
                        DATEFROMPARTS(
                            CASE WHEN s.Season = 0 AND MONTH(GETUTCDATE()) > 3 THEN YEAR(GETUTCDATE()) + 1 ELSE YEAR(GETUTCDATE()) END,
                            CASE s.Season WHEN 0 THEN 3 WHEN 1 THEN 6 WHEN 2 THEN 9 ELSE 12 END,
                            CASE s.Season WHEN 0 THEN 20 WHEN 1 THEN 20 WHEN 2 THEN 22 ELSE 20 END) AS PeriodEnd
                    FROM Quests q
                    INNER JOIN SeasonalQuest_Seasons s ON s.QuestId = q.Id
                    WHERE q.QuestType = 'Seasonal' AND q.Schedule_Unit = 'Year'
                )
                INSERT INTO QuestOccurrences
                    (QuestId, PeriodStart, PeriodEnd, TargetAmount, Progress, CompletedAt, IsBackfilled,
                     RewardGrantedAt, XpAwarded, CoinsAwarded)
                SELECT
                    seasons.QuestId, seasons.PeriodStart, seasons.PeriodEnd, 1,
                    CASE WHEN seasons.IsCompleted = 1 THEN 1 ELSE 0 END,
                    CASE WHEN seasons.IsCompleted = 1 THEN COALESCE(seasons.LastCompletedAt, seasons.CreatedAt) ELSE NULL END,
                    0,
                    CASE WHEN seasons.IsCompleted = 1 THEN COALESCE(seasons.LastCompletedAt, seasons.CreatedAt) ELSE NULL END,
                    0, 0
                FROM seasons
                WHERE CAST(GETUTCDATE() AS date) BETWEEN seasons.PeriodStart AND seasons.PeriodEnd
                  AND NOT EXISTS (SELECT 1 FROM QuestOccurrences o WHERE o.QuestId = seasons.QuestId);");
        }
    }
}
