using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Calculators
{
    /// <summary>
    /// Rolls a quest's occurrences up into its cached <see cref="Models.QuestStatistics"/> row.
    /// <para>
    /// A period counts as a failure only once it has fully elapsed relative to the user's local
    /// <c>today</c>; the in-progress period is neither a success nor a failure until completed.
    /// </para>
    /// </summary>
    public static class QuestStatisticsCalculator
    {
        public static QuestStatisticsData Calculate(IEnumerable<QuestOccurrence> occurrences, DateOnly today)
        {
            var data = new QuestStatisticsData();
            var ordered = occurrences.OrderBy(o => o.PeriodStart).ToList();

            foreach (var occurrence in ordered)
                ProcessOccurrenceForCounts(data, occurrence, today);

            CalculateStreaks(data, ordered, today);

            return data;
        }

        private static void ProcessOccurrenceForCounts(QuestStatisticsData data, QuestOccurrence occurrence, DateOnly today)
        {
            data.OccurrenceCount++;

            if (occurrence.WasCompleted)
            {
                data.CompletionCount++;
                data.LastCompletedAt = occurrence.CompletedAt;
            }
            else if (occurrence.HasElapsedOn(today))
            {
                // Only count as a failure once the period is genuinely over.
                data.FailureCount++;
            }
        }

        private static void CalculateStreaks(QuestStatisticsData data, List<QuestOccurrence> ordered, DateOnly today)
        {
            // Elapsed periods, plus the in-progress period when it has already been completed.
            var relevantOccurrences = ordered
                .Where(o => o.HasElapsedOn(today) || o.WasCompleted)
                .ToList();

            if (relevantOccurrences.Count == 0)
                return;

            data.CurrentStreak = CalculateCurrentStreak(relevantOccurrences, today);
            data.LongestStreak = CalculateLongestStreak(relevantOccurrences, today);
        }

        private static int CalculateCurrentStreak(List<QuestOccurrence> relevantOccurrences, DateOnly today)
        {
            int currentStreak = 0;

            // Work backwards from the most recent occurrence.
            for (int i = relevantOccurrences.Count - 1; i >= 0; i--)
            {
                var occurrence = relevantOccurrences[i];

                if (occurrence.WasCompleted)
                    currentStreak++;
                else if (occurrence.HasElapsedOn(today))
                    break; // A genuinely missed period breaks the streak.
            }

            return currentStreak;
        }

        private static int CalculateLongestStreak(List<QuestOccurrence> relevantOccurrences, DateOnly today)
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
                else if (occurrence.HasElapsedOn(today))
                {
                    currentStreak = 0;
                }
            }

            return longestStreak;
        }
    }
}
