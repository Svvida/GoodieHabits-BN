namespace Application.Quests.Dtos
{
    public record RepeatableQuestStatisticsDto(int CompletionCount, int FailureCount, int OccurrenceCount, int CurrentStreak, int LongestStreak);
}
