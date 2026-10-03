using Domain.ValueObjects;

namespace Application.Quests.Queries.GetQuestAnalytics
{
    /// <param name="StreakUnit">
    /// What one unit of a streak means here — a streak of 5 on a weekly habit is five weeks, not five days,
    /// and a UI showing "🔥 5" has no way to know that otherwise.
    /// </param>
    /// <param name="Range">Metrics restricted to the requested window.</param>
    /// <param name="Lifetime">
    /// The quest's cached all-time statistics. Kept separate because a windowed "current streak" and a
    /// lifetime one are different numbers, and conflating them is how streak displays go wrong.
    /// </param>
    public record GetQuestAnalyticsResponse(
        int QuestId,
        string Title,
        string StreakUnit,
        DateOnly From,
        DateOnly To,
        string Granularity,
        QuestAnalyticsSummary Range,
        LifetimeQuestStatsDto? Lifetime,
        IReadOnlyList<QuestCalendarEntry> Calendar,
        IReadOnlyList<QuestTrendBucket> Trend,
        IReadOnlyList<QuestWeekdayBreakdown> ByWeekday,
        IReadOnlyList<QuestHourBreakdown> ByHourOfDay);

    public record LifetimeQuestStatsDto(
        int CompletionCount,
        int FailureCount,
        int PartialCount,
        int OccurrenceCount,
        int TotalCompletions,
        int CurrentStreak,
        int LongestStreak,
        double? CompletionRate,
        DateTime? LastCompletedAtUtc);
}
