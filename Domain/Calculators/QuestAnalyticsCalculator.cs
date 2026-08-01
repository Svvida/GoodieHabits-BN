using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Calculators
{
    /// <summary>
    /// Turns quest occurrence periods into the numbers the analytics endpoints report.
    /// <para>
    /// Pure calendar maths over <see cref="DateOnly"/> periods — no timezone conversion, no database.
    /// Every entry point takes the user's local <c>today</c> so "has this period elapsed yet?" is decided
    /// consistently in one place.
    /// </para>
    /// </summary>
    public static class QuestAnalyticsCalculator
    {
        public static QuestPeriodOutcomeEnum OutcomeOf(QuestOccurrence occurrence, DateOnly today)
        {
            if (occurrence.WasCompleted)
                return QuestPeriodOutcomeEnum.Completed;

            return occurrence.HasElapsedOn(today)
                ? QuestPeriodOutcomeEnum.Missed
                : QuestPeriodOutcomeEnum.Pending;
        }

        /// <summary>
        /// Headline metrics over the supplied occurrences. Streaks are relative to this set, so when the
        /// caller passes a date-bounded window the streaks describe that window, not the quest's lifetime.
        /// </summary>
        public static QuestAnalyticsSummary Summarize(IEnumerable<QuestOccurrence> occurrences, DateOnly today)
        {
            var ordered = occurrences.OrderBy(o => o.PeriodStart).ToList();
            if (ordered.Count == 0)
                return QuestAnalyticsSummary.Empty;

            int completed = 0, missed = 0, pending = 0;
            DateTime? lastCompletedAt = null;

            foreach (var occurrence in ordered)
            {
                switch (OutcomeOf(occurrence, today))
                {
                    case QuestPeriodOutcomeEnum.Completed:
                        completed++;
                        if (occurrence.CompletedAt.HasValue &&
                            (lastCompletedAt is null || occurrence.CompletedAt > lastCompletedAt))
                        {
                            lastCompletedAt = occurrence.CompletedAt;
                        }
                        break;
                    case QuestPeriodOutcomeEnum.Missed:
                        missed++;
                        break;
                    default:
                        pending++;
                        break;
                }
            }

            int evaluated = completed + missed;

            return new QuestAnalyticsSummary(
                TotalPeriods: ordered.Count,
                CompletedPeriods: completed,
                MissedPeriods: missed,
                PendingPeriods: pending,
                EvaluatedPeriods: evaluated,
                CompletionRate: Rate(completed, evaluated),
                CurrentStreak: CurrentStreak(ordered, today),
                LongestStreak: LongestStreak(ordered, today),
                LastCompletedAtUtc: lastCompletedAt);
        }

        public static IReadOnlyList<QuestCalendarEntry> ToCalendar(IEnumerable<QuestOccurrence> occurrences, DateOnly today)
        {
            return [.. occurrences
                .OrderBy(o => o.PeriodStart)
                .Select(o => new QuestCalendarEntry(
                    o.PeriodStart,
                    o.PeriodEnd,
                    OutcomeOf(o, today),
                    o.CompletedAt,
                    o.IsBackfilled))];
        }

        /// <summary>
        /// Groups periods into a trend series by the bucket their <c>PeriodStart</c> falls in. Buckets with
        /// no occurrences are omitted rather than reported as 0% — an absent bucket means "nothing was
        /// scheduled", which is not the same as "everything was missed".
        /// </summary>
        public static IReadOnlyList<QuestTrendBucket> Bucket(
            IEnumerable<QuestOccurrence> occurrences,
            AnalyticsGranularityEnum granularity,
            DateOnly today)
        {
            return [.. occurrences
                .GroupBy(o => BucketStartFor(o.PeriodStart, granularity))
                .OrderBy(group => group.Key)
                .Select(group =>
                {
                    int completed = group.Count(o => OutcomeOf(o, today) == QuestPeriodOutcomeEnum.Completed);
                    int missed = group.Count(o => OutcomeOf(o, today) == QuestPeriodOutcomeEnum.Missed);
                    int evaluated = completed + missed;

                    return new QuestTrendBucket(
                        group.Key,
                        BucketEndFor(group.Key, granularity),
                        completed,
                        missed,
                        evaluated,
                        Rate(completed, evaluated));
                })];
        }

        /// <summary>
        /// Per-weekday performance. Only single-day periods are considered, so this stays meaningful for
        /// daily and weekly quests and simply returns nothing for monthly ones, whose periods span a range.
        /// </summary>
        public static IReadOnlyList<QuestWeekdayBreakdown> ByWeekday(IEnumerable<QuestOccurrence> occurrences, DateOnly today)
        {
            return [.. occurrences
                .Where(o => o.PeriodStart == o.PeriodEnd)
                .GroupBy(o => (WeekdayEnum)o.PeriodStart.DayOfWeek)
                .OrderBy(group => group.Key)
                .Select(group =>
                {
                    int completed = group.Count(o => OutcomeOf(o, today) == QuestPeriodOutcomeEnum.Completed);
                    int missed = group.Count(o => OutcomeOf(o, today) == QuestPeriodOutcomeEnum.Missed);
                    int evaluated = completed + missed;

                    return new QuestWeekdayBreakdown(group.Key, completed, missed, evaluated, Rate(completed, evaluated));
                })];
        }

        private static double? Rate(int completed, int evaluated) =>
            evaluated == 0 ? null : Math.Round((double)completed / evaluated, 4);

        private static int CurrentStreak(List<QuestOccurrence> ordered, DateOnly today)
        {
            int streak = 0;

            for (int i = ordered.Count - 1; i >= 0; i--)
            {
                var outcome = OutcomeOf(ordered[i], today);

                if (outcome == QuestPeriodOutcomeEnum.Completed)
                    streak++;
                else if (outcome == QuestPeriodOutcomeEnum.Missed)
                    break;
                // Pending periods are skipped: an unfinished day neither extends nor breaks a streak.
            }

            return streak;
        }

        private static int LongestStreak(List<QuestOccurrence> ordered, DateOnly today)
        {
            int longest = 0, current = 0;

            foreach (var occurrence in ordered)
            {
                var outcome = OutcomeOf(occurrence, today);

                if (outcome == QuestPeriodOutcomeEnum.Completed)
                {
                    current++;
                    longest = Math.Max(longest, current);
                }
                else if (outcome == QuestPeriodOutcomeEnum.Missed)
                {
                    current = 0;
                }
            }

            return longest;
        }

        private static DateOnly BucketStartFor(DateOnly date, AnalyticsGranularityEnum granularity) => granularity switch
        {
            AnalyticsGranularityEnum.Day => date,
            // ISO weeks start on Monday.
            AnalyticsGranularityEnum.Week => date.AddDays(-(((int)date.DayOfWeek + 6) % 7)),
            AnalyticsGranularityEnum.Month => new DateOnly(date.Year, date.Month, 1),
            _ => date
        };

        private static DateOnly BucketEndFor(DateOnly bucketStart, AnalyticsGranularityEnum granularity) => granularity switch
        {
            AnalyticsGranularityEnum.Day => bucketStart,
            AnalyticsGranularityEnum.Week => bucketStart.AddDays(6),
            AnalyticsGranularityEnum.Month => bucketStart.AddMonths(1).AddDays(-1),
            _ => bucketStart
        };
    }
}
