namespace Application.Dtos.Quests
{
    public class RepeatableQuestStatisticsDto
    {
        public int CompletionCount { get; set; }
        public int FailureCount { get; set; }
        public int OccurrenceCount { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastCompletedAt { get; set; } // Nullable to handle quests that have never been completed
    }
}
