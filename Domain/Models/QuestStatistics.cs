using Domain.ValueObjects;

namespace Domain.Models
{
    /// <summary>
    /// A denormalized cache of a quest's lifetime figures, for list views. It is not the query path for
    /// analytics — those aggregate the periods directly — and it is kept fresh by the per-user maintenance
    /// pass and by every completion.
    /// </summary>
    public class QuestStatistics
    {
        public int Id { get; private set; }
        public int QuestId { get; private set; }

        public int CompletionCount { get; private set; }
        public int FailureCount { get; private set; }

        /// <summary>How many of the failures had some progress recorded against them.</summary>
        public int PartialCount { get; private set; }

        public int OccurrenceCount { get; private set; }

        /// <summary>Individual taps rather than completed periods — "you brushed 43 times".</summary>
        public int TotalCompletions { get; private set; }

        public int CurrentStreak { get; private set; }
        public int LongestStreak { get; private set; }

        public DateTime? LastCompletedAt { get; private set; }

        public Quest Quest { get; private set; } = null!;

        protected QuestStatistics() { }

        private QuestStatistics(Quest quest) => Quest = quest;

        public static QuestStatistics Create(Quest quest) => new(quest);

        public void UpdateFrom(QuestStatisticsData newStats)
        {
            CompletionCount = newStats.CompletionCount;
            FailureCount = newStats.FailureCount;
            PartialCount = newStats.PartialCount;
            OccurrenceCount = newStats.OccurrenceCount;
            TotalCompletions = newStats.TotalCompletions;
            CurrentStreak = newStats.CurrentStreak;
            LongestStreak = newStats.LongestStreak;
            LastCompletedAt = newStats.LastCompletedAt;
        }
    }
}
