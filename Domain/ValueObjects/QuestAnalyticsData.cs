using Domain.Enums;

namespace Domain.ValueObjects
{
    /// <summary>
    /// Headline metrics for a set of quest periods.
    /// <para>
    /// <paramref name="CompletionRate"/> divides by <paramref name="EvaluatedPeriods"/> (periods that have
    /// fully elapsed, plus any already completed) rather than by <paramref name="TotalPeriods"/> — an
    /// in-progress day is not a failure yet, and counting it as one makes today's percentage dip for no
    /// reason. It is <c>null</c> when nothing has been evaluated, so the UI can say "no data" instead of 0%.
    /// </para>
    /// <para>
    /// <paramref name="ProgressRate"/> is the partial-credit companion: how much of what was asked for was
    /// actually done. For a "brush twice a day" habit, <c>CompletionRate</c> answers "on how many days did I
    /// do both?" and <c>ProgressRate</c> answers "what share of all the brushings did I do?".
    /// </para>
    /// </summary>
    public record QuestAnalyticsSummary(
        int TotalPeriods,
        int CompletedPeriods,
        int MissedPeriods,
        int PartialPeriods,
        int PendingPeriods,
        int SkippedPeriods,
        int EvaluatedPeriods,
        double? CompletionRate,
        double? ProgressRate,
        int TotalCompletions,
        int CurrentStreak,
        int LongestStreak,
        DateTime? LastCompletedAtUtc)
    {
        public static QuestAnalyticsSummary Empty { get; } = new(0, 0, 0, 0, 0, 0, 0, null, null, 0, 0, 0, null);
    }

    /// <summary>One cell of a calendar or heatmap view.</summary>
    public record QuestCalendarEntry(
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        QuestPeriodOutcomeEnum Outcome,
        decimal Progress,
        decimal Target,
        DateTime? CompletedAtUtc,
        bool IsBackfilled);

    /// <summary>One point of a trend series (a week, month, or day).</summary>
    public record QuestTrendBucket(
        DateOnly BucketStart,
        DateOnly BucketEnd,
        int CompletedPeriods,
        int MissedPeriods,
        int EvaluatedPeriods,
        double? CompletionRate);

    /// <summary>
    /// Per-weekday performance — answers "which day do I keep dropping this habit?".
    /// <para>
    /// Counted from the completion log rather than from single-day periods, so it stays meaningful for a
    /// "three times a week, any days" habit, where the period is the week but the doing happens on days.
    /// </para>
    /// </summary>
    /// <param name="DaysInRange">
    /// How many times this weekday occurred in the requested window. Without it the counts cannot be
    /// normalised — three Mondays done means something different across four Mondays than across thirteen.
    /// </param>
    /// <param name="DaysScheduled">
    /// How many of those the quest was actually due on. <c>null</c> when the schedule does not pin
    /// weekdays at all (a Week/Month/Year unit), where "scheduled on a Tuesday" is not a thing —
    /// there <see cref="DaysInRange"/> is the only honest denominator.
    /// </param>
    public record QuestWeekdayBreakdown(
        WeekdayEnum Weekday,
        int Completions,
        int DaysWithActivity,
        int DaysInRange,
        int? DaysScheduled);

    /// <summary>
    /// When in the day the habit actually happens, from each tap's snapshotted local time. This is what
    /// makes one "brush teeth twice a day" quest as informative as two separate morning/evening quests.
    /// </summary>
    public record QuestHourBreakdown(int Hour, int Completions);
}
