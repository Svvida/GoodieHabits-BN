namespace Domain.ValueObjects
{
    /// <summary>
    /// The denormalized roll-up cached on <see cref="Models.QuestStatistics"/> for list views. The analytics
    /// endpoints aggregate the periods themselves rather than reading this.
    /// </summary>
    public class QuestStatisticsData
    {
        /// <summary>Periods whose target was reached.</summary>
        public int CompletionCount { get; set; }

        /// <summary>Elapsed periods that fell short, partial ones included.</summary>
        public int FailureCount { get; set; }

        /// <summary>Of those failures, how many had *some* progress. Reported so "missed" can mean missed.</summary>
        public int PartialCount { get; set; }

        public int OccurrenceCount { get; set; }

        /// <summary>Individual taps, off-schedule ones included — always at least <see cref="CompletionCount"/>.</summary>
        public int TotalCompletions { get; set; }

        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastCompletedAt { get; set; }
    }
}
