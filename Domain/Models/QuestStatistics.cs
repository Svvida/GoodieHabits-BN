namespace Domain.Models
{
    public class QuestStatistics
    {
        public int Id { get; private set; }
        public int QuestId { get; private set; }

        public int CompletionCount { get; private set; } = 0;
        public int FailureCount { get; private set; } = 0;
        public int OccurrenceCount { get; private set; } = 0;
        public int CurrentStreak { get; private set; } = 0;
        public int LongestStreak { get; private set; } = 0;

        public DateTime? LastCompletedAt { get; private set; } = null;

        public Quest Quest { get; private set; } = null!;

        protected QuestStatistics() { }
        private QuestStatistics(Quest quest)
        {
            Quest = quest;
        }

        public static QuestStatistics Create(Quest quest)
        {
            return new QuestStatistics(quest);
        }

        public void UpdateFrom(QuestStatistics newStats)
        {
            CompletionCount = newStats.CompletionCount;
            FailureCount = newStats.FailureCount;
            OccurrenceCount = newStats.OccurrenceCount;
            CurrentStreak = newStats.CurrentStreak;
            LongestStreak = newStats.LongestStreak;
            LastCompletedAt = newStats.LastCompletedAt;
        }
    }
}
