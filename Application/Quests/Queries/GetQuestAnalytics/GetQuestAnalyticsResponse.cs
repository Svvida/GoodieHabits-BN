using Domain.ValueObjects;

namespace Application.Quests.Queries.GetQuestAnalytics
{
    /// <param name="Range">Metrics restricted to the requested window.</param>
    /// <param name="Lifetime">
    /// The quest's cached all-time statistics. Kept separate because a windowed "current streak" and a
    /// lifetime one are different numbers and conflating them is how streak displays go wrong.
    /// </param>
    public record GetQuestAnalyticsResponse(
        int QuestId,
        string QuestType,
        string Title,
        DateOnly From,
        DateOnly To,
        string Granularity,
        QuestAnalyticsSummary Range,
        LifetimeQuestStatsDto? Lifetime,
        IReadOnlyList<QuestCalendarEntry> Calendar,
        IReadOnlyList<QuestTrendBucket> Trend,
        IReadOnlyList<QuestWeekdayBreakdown> ByWeekday);

    public record LifetimeQuestStatsDto(
        int CompletionCount,
        int FailureCount,
        int OccurrenceCount,
        int CurrentStreak,
        int LongestStreak,
        double? CompletionRate,
        DateTime? LastCompletedAtUtc);
}
