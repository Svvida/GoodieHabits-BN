using Domain.Enums;

namespace Domain.ValueObjects
{
    /// <summary>
    /// Headline metrics for a set of quest occurrence periods.
    /// <para>
    /// <paramref name="CompletionRate"/> deliberately divides by <paramref name="EvaluatedPeriods"/>
    /// (periods that have fully elapsed, plus any already completed) rather than by
    /// <paramref name="TotalPeriods"/> — an in-progress day is not a failure yet, and counting it as one
    /// makes today's percentage dip for no reason. It is <c>null</c> when nothing has been evaluated,
    /// so the UI can say "no data" instead of showing 0%.
    /// </para>
    /// </summary>
    public record QuestAnalyticsSummary(
        int TotalPeriods,
        int CompletedPeriods,
        int MissedPeriods,
        int PendingPeriods,
        int EvaluatedPeriods,
        double? CompletionRate,
        int CurrentStreak,
        int LongestStreak,
        DateTime? LastCompletedAtUtc)
    {
        public static QuestAnalyticsSummary Empty { get; } = new(0, 0, 0, 0, 0, null, 0, 0, null);
    }

    /// <summary>One cell of a calendar or heatmap view.</summary>
    public record QuestCalendarEntry(
        DateOnly PeriodStart,
        DateOnly PeriodEnd,
        QuestPeriodOutcomeEnum Outcome,
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

    /// <summary>Per-weekday performance — answers "which day do I keep dropping this habit?".</summary>
    public record QuestWeekdayBreakdown(
        WeekdayEnum Weekday,
        int CompletedPeriods,
        int MissedPeriods,
        int EvaluatedPeriods,
        double? CompletionRate);
}
