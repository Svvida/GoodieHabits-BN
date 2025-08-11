namespace Domain.ValueObjects
{
    public class QuestStatisticsData
    {
        public int CompletionCount { get; set; } = 0;
        public int FailureCount { get; set; } = 0;
        public int OccurrenceCount { get; set; } = 0;
        public int CurrentStreak { get; set; } = 0;
        public int LongestStreak { get; set; } = 0;
        public DateTime? LastCompletedAt { get; set; } = null;

    }
}
