﻿namespace Domain.Models
{
    public class QuestStatistics
    {
        public int Id { get; set; }
        public int QuestId { get; set; }

        public int CompletionCount { get; set; } = 0;
        public int FailureCount { get; set; } = 0;
        public int OccurrenceCount { get; set; } = 0;
        public int CurrentStreak { get; set; } = 0;
        public int LongestStreak { get; set; } = 0;

        public DateTime? LastCompletedAt { get; set; } = null;

        public Quest Quest { get; set; } = null!;

        public QuestStatistics() { }
        public QuestStatistics(int questId)
        {
            QuestId = questId;
        }

        public void UpdateFrom(QuestStatistics source)
        {
            CompletionCount = source.CompletionCount;
            FailureCount = source.FailureCount;
            OccurrenceCount = source.OccurrenceCount;
            CurrentStreak = source.CurrentStreak;
            LongestStreak = source.LongestStreak;
            LastCompletedAt = source.LastCompletedAt;
        }
    }
}
