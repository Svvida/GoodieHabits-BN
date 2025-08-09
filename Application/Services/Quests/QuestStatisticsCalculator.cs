using Application.Common.Interfaces.Quests;
using Domain.Models;
using NodaTime;

namespace Application.Services.Quests
{
    public class QuestStatisticsCalculator(IClock clock) : IQuestStatisticsCalculator
    {
        public QuestStatistics Calculate(IEnumerable<QuestOccurrence> occurrences)
        {
            var stats = new QuestStatistics();
            var now = clock.GetCurrentInstant().ToDateTimeUtc();
            var ordered = occurrences.OrderBy(o => o.OccurrenceStart).ToList();

            // Process all occurrences for basic counts
            foreach (var occurrence in ordered)
            {
                ProcessOccurrenceForCounts(stats, occurrence, now);
            }

            // Calculate streaks separately, considering only past occurrences
            CalculateStreaks(stats, ordered, now);

            return stats;
        }

        private static void ProcessOccurrenceForCounts(QuestStatistics stats, QuestOccurrence occurrence, DateTime now)
        {
            stats.OccurrenceCount++;

            if (occurrence.WasCompleted)
            {
                stats.CompletionCount++;
                stats.LastCompletedAt = occurrence.CompletedAt;
            }
            else if (occurrence.OccurrenceEnd <= now)
            {
                // Only count as failure if the occurrence window has passed
                stats.FailureCount++;
            }
        }

        private static void CalculateStreaks(QuestStatistics stats, List<QuestOccurrence> ordered, DateTime now)
        {
            // Include past occurrences AND current occurrence that is completed
            var relevantOccurrences = ordered
                .Where(o => o.OccurrenceEnd <= now || o.WasCompleted)
                .ToList();

            if (relevantOccurrences.Count == 0)
            {
                return;
            }

            // Calculate current streak (working backwards from most recent)
            stats.CurrentStreak = CalculateCurrentStreak(relevantOccurrences, now);

            // Calculate longest streak
            stats.LongestStreak = CalculateLongestStreak(relevantOccurrences, now);
        }

        private static int CalculateCurrentStreak(List<QuestOccurrence> relevantOccurrences, DateTime now)
        {
            int currentStreak = 0;

            // Work backwards from the most recent occurrence
            for (int i = relevantOccurrences.Count - 1; i >= 0; i--)
            {
                var occurrence = relevantOccurrences[i];

                if (occurrence.WasCompleted)
                {
                    currentStreak++;
                }
                else if (occurrence.OccurrenceEnd <= now)
                {
                    // Only break streak for missed opportunities (past occurrences that weren't completed)
                    break;
                }
            }

            return currentStreak;
        }

        private static int CalculateLongestStreak(List<QuestOccurrence> relevantOccurrences, DateTime now)
        {
            int longestStreak = 0;
            int currentStreak = 0;

            foreach (var occurrence in relevantOccurrences)
            {
                if (occurrence.WasCompleted)
                {
                    currentStreak++;
                    longestStreak = Math.Max(longestStreak, currentStreak);
                }
                else if (occurrence.OccurrenceEnd <= now)
                {
                    // Only reset streak for actual missed opportunities
                    currentStreak = 0;
                }
                // Skip incomplete future occurrences - they don't affect streak calculation
            }

            return longestStreak;
        }
    }
}
