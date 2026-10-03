namespace Application.Quests.Dtos
{
    /// <summary>
    /// A quest's cached lifetime figures. Null on quests with no recurrence — a one-time quest has no streak.
    /// </summary>
    /// <param name="FailureCount">Elapsed periods that fell short, <paramref name="PartialCount"/> included.</param>
    /// <param name="TotalCompletions">Individual taps, not completed periods.</param>
    public record RepeatableQuestStatisticsDto(
        int CompletionCount,
        int FailureCount,
        int PartialCount,
        int OccurrenceCount,
        int TotalCompletions,
        int CurrentStreak,
        int LongestStreak,
        DateTime? LastCompletedAt);
}
