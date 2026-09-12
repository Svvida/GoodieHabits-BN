using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Calculators
{
    /// <summary>
    /// Rolls a quest's periods up into its cached <see cref="Models.QuestStatistics"/> row.
    /// <para>
    /// A period counts as a failure only once it has fully elapsed relative to the user's local
    /// <c>today</c>; the in-progress period is neither a success nor a failure until its target is reached.
    /// A skipped period is neither, ever.
    /// </para>
    /// <para>
    /// Outcomes come from <see cref="QuestOccurrence.OutcomeOn"/> so this and the analytics endpoints can
    /// never disagree about what "missed" means.
    /// </para>
    /// </summary>
    public static class QuestStatisticsCalculator
    {
        /// <param name="totalCompletions">
        /// Taken from the quest rather than summed over the periods: an off-schedule completion belongs to no
        /// period at all, so summing would quietly undercount exactly the taps worth noticing.
        /// </param>
        public static QuestStatisticsData Calculate(
            IEnumerable<QuestOccurrence> occurrences,
            DateOnly today,
            int totalCompletions = 0)
        {
            var data = new QuestStatisticsData { TotalCompletions = totalCompletions };
            var ordered = occurrences.OrderBy(o => o.PeriodStart).ToList();

            foreach (var occurrence in ordered)
            {
                data.OccurrenceCount++;

                switch (occurrence.OutcomeOn(today))
                {
                    case QuestPeriodOutcomeEnum.Completed:
                        data.CompletionCount++;
                        if (occurrence.CompletedAt > data.LastCompletedAt || data.LastCompletedAt is null)
                            data.LastCompletedAt = occurrence.CompletedAt;
                        break;

                    case QuestPeriodOutcomeEnum.Partial:
                        data.PartialCount++;
                        data.FailureCount++;
                        break;

                    case QuestPeriodOutcomeEnum.Missed:
                        data.FailureCount++;
                        break;
                }
            }

            data.CurrentStreak = CurrentStreak(ordered, today);
            data.LongestStreak = LongestStreak(ordered, today);

            return data;
        }

        private static int CurrentStreak(List<QuestOccurrence> ordered, DateOnly today)
        {
            int streak = 0;

            // Work backwards from the most recent period.
            for (int i = ordered.Count - 1; i >= 0; i--)
            {
                switch (ordered[i].OutcomeOn(today))
                {
                    case QuestPeriodOutcomeEnum.Completed:
                        streak++;
                        break;

                    case QuestPeriodOutcomeEnum.Missed:
                    case QuestPeriodOutcomeEnum.Partial:
                        return streak;

                    // Pending and Skipped periods neither extend nor break a streak.
                    default:
                        break;
                }
            }

            return streak;
        }

        private static int LongestStreak(List<QuestOccurrence> ordered, DateOnly today)
        {
            int longest = 0, current = 0;

            foreach (var occurrence in ordered)
            {
                switch (occurrence.OutcomeOn(today))
                {
                    case QuestPeriodOutcomeEnum.Completed:
                        current++;
                        longest = Math.Max(longest, current);
                        break;

                    case QuestPeriodOutcomeEnum.Missed:
                    case QuestPeriodOutcomeEnum.Partial:
                        current = 0;
                        break;

                    default:
                        break;
                }
            }

            return longest;
        }
    }
}
