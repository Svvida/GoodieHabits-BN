using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Calculators
{
    /// <summary>
    /// Turns quest periods and completions into the numbers the analytics endpoints report.
    /// <para>
    /// Pure calendar maths — no timezone conversion, no database. Every entry point takes the user's local
    /// <c>today</c> so "has this period elapsed yet?" is decided consistently in one place, and outcomes come
    /// from <see cref="QuestOccurrence.OutcomeOn"/> so they match the cached statistics exactly.
    /// </para>
    /// </summary>
    public static class QuestAnalyticsCalculator
    {
        public static QuestPeriodOutcomeEnum OutcomeOf(QuestOccurrence occurrence, DateOnly today) =>
            occurrence.OutcomeOn(today);

        /// <summary>
        /// Headline metrics over the supplied periods. Streaks are relative to this set, so when the caller
        /// passes a date-bounded window the streaks describe that window, not the quest's lifetime.
        /// </summary>
        /// <param name="totalCompletions">
        /// Supplied by the caller from the completion log rather than counted off the periods: an
        /// off-schedule completion belongs to no period, so counting through them undercounts precisely the
        /// taps worth seeing — and the periods are usually loaded without their completions anyway.
        /// </param>
        public static QuestAnalyticsSummary Summarize(
            IEnumerable<QuestOccurrence> occurrences,
            DateOnly today,
            int totalCompletions = 0)
        {
            var ordered = occurrences.OrderBy(o => o.PeriodStart).ToList();
            if (ordered.Count == 0)
                return QuestAnalyticsSummary.Empty;

            int completed = 0, missed = 0, partial = 0, pending = 0, skipped = 0;
            decimal progressAchieved = 0m, progressAsked = 0m;
            DateTime? lastCompletedAt = null;

            foreach (var occurrence in ordered)
            {
                var outcome = occurrence.OutcomeOn(today);

                switch (outcome)
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
                    case QuestPeriodOutcomeEnum.Partial:
                        partial++;
                        break;
                    case QuestPeriodOutcomeEnum.Skipped:
                        skipped++;
                        break;
                    default:
                        pending++;
                        break;
                }

                if (outcome is QuestPeriodOutcomeEnum.Pending or QuestPeriodOutcomeEnum.Skipped)
                    continue;

                // Credit is capped at the target: going over does not paper over another period's shortfall.
                progressAchieved += Math.Min(occurrence.Progress, occurrence.TargetAmount);
                progressAsked += occurrence.TargetAmount;
            }

            // Partial periods are failures for rate purposes; they are reported separately only so the UI can
            // show "1 of 2" instead of plain red.
            int evaluated = completed + missed + partial;

            return new QuestAnalyticsSummary(
                TotalPeriods: ordered.Count,
                CompletedPeriods: completed,
                MissedPeriods: missed,
                PartialPeriods: partial,
                PendingPeriods: pending,
                SkippedPeriods: skipped,
                EvaluatedPeriods: evaluated,
                CompletionRate: Rate(completed, evaluated),
                ProgressRate: Ratio(progressAchieved, progressAsked),
                TotalCompletions: totalCompletions,
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
                    o.OutcomeOn(today),
                    o.Progress,
                    o.TargetAmount,
                    o.CompletedAt,
                    o.IsBackfilled))];
        }

        /// <summary>
        /// Groups periods into a trend series by the bucket their <c>PeriodStart</c> falls in. Buckets with
        /// no periods are omitted rather than reported as 0% — an absent bucket means "nothing was
        /// scheduled", which is not the same as "everything was missed".
        /// </summary>
        public static IReadOnlyList<QuestTrendBucket> Bucket(
            IEnumerable<QuestOccurrence> occurrences,
            AnalyticsGranularityEnum granularity,
            DateOnly today,
            DayOfWeek weekStartsOn = DayOfWeek.Monday)
        {
            return [.. occurrences
                .GroupBy(o => BucketStartFor(o.PeriodStart, granularity, weekStartsOn))
                .OrderBy(group => group.Key)
                .Select(group =>
                {
                    int completed = group.Count(o => o.OutcomeOn(today) == QuestPeriodOutcomeEnum.Completed);
                    int missed = group.Count(o => o.OutcomeOn(today) is QuestPeriodOutcomeEnum.Missed or QuestPeriodOutcomeEnum.Partial);
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
        /// Per-weekday activity, from the completion log. Reading the log rather than the periods is what
        /// makes this work for every schedule: a "three times a week, any days" habit has one period per
        /// week, but the interesting question is still which days the user actually did it on.
        /// <para>
        /// A row is emitted for every weekday that was either done or scheduled, so a weekday the user
        /// keeps missing still appears — with zero completions against a non-zero denominator, which is
        /// the whole point of the widget.
        /// </para>
        /// </summary>
        public static IReadOnlyList<QuestWeekdayBreakdown> ByWeekday(
            IEnumerable<QuestCompletion> completions,
            IEnumerable<QuestOccurrence> occurrences,
            DateOnly from,
            DateOnly to)
        {
            var completionsByDay = completions
                .GroupBy(c => (WeekdayEnum)c.CompletedOn.DayOfWeek)
                .ToDictionary(group => group.Key, group => group.ToList());

            // Only single-day periods say anything about a weekday. A Week or Month period spans several,
            // so for those schedules "how many Tuesdays was I due?" has no answer and stays null.
            var singleDayPeriods = occurrences.Where(o => o.PeriodStart == o.PeriodEnd).ToList();
            bool schedulePinsWeekdays = singleDayPeriods.Count > 0;

            var scheduledByDay = singleDayPeriods
                .GroupBy(o => (WeekdayEnum)o.PeriodStart.DayOfWeek)
                .ToDictionary(group => group.Key, group => group.Count());

            var breakdown = new List<QuestWeekdayBreakdown>();

            foreach (WeekdayEnum weekday in Enum.GetValues<WeekdayEnum>())
            {
                var done = completionsByDay.GetValueOrDefault(weekday) ?? [];
                int scheduled = scheduledByDay.GetValueOrDefault(weekday);

                if (done.Count == 0 && scheduled == 0)
                    continue;

                breakdown.Add(new QuestWeekdayBreakdown(
                    weekday,
                    done.Count,
                    done.Select(c => c.CompletedOn).Distinct().Count(),
                    OccurrencesOfWeekdayBetween(weekday, from, to),
                    schedulePinsWeekdays ? scheduled : null));
            }

            return breakdown;
        }

        /// <summary>How many times one weekday falls inside an inclusive date range.</summary>
        private static int OccurrencesOfWeekdayBetween(WeekdayEnum weekday, DateOnly from, DateOnly to)
        {
            if (to < from)
                return 0;

            int totalDays = to.DayNumber - from.DayNumber + 1;
            int wholeWeeks = totalDays / 7;
            int remainder = totalDays % 7;

            // The leftover days run forward from `from`; count how many of them land on this weekday.
            for (int offset = 0; offset < remainder; offset++)
            {
                if ((WeekdayEnum)from.AddDays(offset).DayOfWeek == weekday)
                    return wholeWeeks + 1;
            }

            return wholeWeeks;
        }

        /// <summary>
        /// Completions by local hour of day. Completions with no recorded local time — rows migrated from
        /// before it was captured — are skipped rather than guessed at.
        /// </summary>
        public static IReadOnlyList<QuestHourBreakdown> ByHourOfDay(IEnumerable<QuestCompletion> completions)
        {
            return [.. completions
                .Where(c => c.LocalTime.HasValue)
                .GroupBy(c => c.LocalTime!.Value.Hour)
                .OrderBy(group => group.Key)
                .Select(group => new QuestHourBreakdown(group.Key, group.Count()))];
        }

        private static double? Rate(int completed, int evaluated) =>
            evaluated == 0 ? null : Math.Round((double)completed / evaluated, 4);

        private static double? Ratio(decimal achieved, decimal asked) =>
            asked == 0m ? null : Math.Round((double)(achieved / asked), 4);

        private static int CurrentStreak(List<QuestOccurrence> ordered, DateOnly today)
        {
            int streak = 0;

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

        private static DateOnly BucketStartFor(DateOnly date, AnalyticsGranularityEnum granularity, DayOfWeek weekStartsOn) => granularity switch
        {
            AnalyticsGranularityEnum.Day => date,
            AnalyticsGranularityEnum.Week => QuestPeriodCalculator.StartOfWeek(date, weekStartsOn),
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
