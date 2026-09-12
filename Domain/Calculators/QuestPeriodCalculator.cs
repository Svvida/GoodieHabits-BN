using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Calculators
{
    /// <summary>
    /// Turns a <see cref="QuestSchedule"/> into the local calendar periods a quest owns. This is the single
    /// implementation of "when is this quest due", replacing <c>QuestWindowCalculator</c>,
    /// <c>NextResetDateCalculator</c>, the per-type <c>WHERE</c> in the active-quests query and
    /// <c>SeasonHelper</c>.
    /// <para>
    /// Pure calendar arithmetic — no timezone conversion, no database. The caller has already resolved
    /// "which local day is it for this user" through <see cref="Models.UserProfile.LocalDateOn"/>, which is
    /// what keeps periods stable when the user's timezone changes.
    /// </para>
    /// </summary>
    public static class QuestPeriodCalculator
    {
        /// <summary>The period covering <paramref name="date"/>, or null when the quest is not due then.</summary>
        public static QuestPeriodWindow? PeriodCovering(
            QuestSchedule schedule,
            QuestScheduleBounds bounds,
            DayOfWeek weekStartsOn,
            DateOnly date)
        {
            if (!bounds.IsActiveOn(date))
                return null;

            foreach (var window in EnumerateCanonicalWindows(schedule, bounds, weekStartsOn, date, date))
            {
                if (!window.Covers(date) || !IsAligned(schedule, bounds, weekStartsOn, window))
                    continue;

                return Clip(window, bounds);
            }

            return null;
        }

        /// <summary>
        /// Every period overlapping <c>[from, to]</c>. Periods are returned whole, not cut to the requested
        /// range — a monthly period straddling the edge still describes its real calendar span, mirroring
        /// how the occurrence repository queries overlap rather than containment.
        /// </summary>
        public static IReadOnlyList<QuestPeriodWindow> PeriodsBetween(
            QuestSchedule schedule,
            QuestScheduleBounds bounds,
            DayOfWeek weekStartsOn,
            DateOnly from,
            DateOnly to)
        {
            if (to < from)
                return [];

            // Never generate outside the quest's own lifetime.
            if (from < bounds.EffectiveFrom)
                from = bounds.EffectiveFrom;

            if (bounds.ActiveTo.HasValue && to > bounds.ActiveTo.Value)
                to = bounds.ActiveTo.Value;

            if (to < from)
                return [];

            var periods = new List<QuestPeriodWindow>();

            foreach (var window in EnumerateCanonicalWindows(schedule, bounds, weekStartsOn, from, to))
            {
                if (!IsAligned(schedule, bounds, weekStartsOn, window))
                    continue;

                if (Clip(window, bounds) is QuestPeriodWindow period)
                    periods.Add(period);
            }

            return periods;
        }

        /// <summary>The first day of the week containing <paramref name="date"/>, per the user's week start.</summary>
        public static DateOnly StartOfWeek(DateOnly date, DayOfWeek weekStartsOn) =>
            date.AddDays(-Mod((int)date.DayOfWeek - (int)weekStartsOn, 7));

        // ─────────────────────────── canonical windows ───────────────────────────

        /// <summary>
        /// Every window of the schedule's unit that overlaps the range, before interval alignment and
        /// before clipping. Yielded in calendar order.
        /// </summary>
        private static IEnumerable<QuestPeriodWindow> EnumerateCanonicalWindows(
            QuestSchedule schedule,
            QuestScheduleBounds bounds,
            DayOfWeek weekStartsOn,
            DateOnly from,
            DateOnly to)
        {
            return schedule.Unit switch
            {
                // A non-recurring quest has exactly one period: its whole active range. Built from the
                // bounds rather than clipped down from an open-ended window, so it is never seen as partial
                // and its target is never prorated away.
                PeriodUnitEnum.None => [new QuestPeriodWindow(bounds.EffectiveFrom, bounds.ActiveTo ?? DateOnly.MaxValue)],
                PeriodUnitEnum.Day => DailyWindows(schedule, from, to),
                PeriodUnitEnum.Week => WeeklyWindows(weekStartsOn, from, to),
                PeriodUnitEnum.Month => MonthlyWindows(schedule, from, to),
                PeriodUnitEnum.Year => YearlyWindows(schedule, from, to),
                _ => []
            };
        }

        private static IEnumerable<QuestPeriodWindow> DailyWindows(QuestSchedule schedule, DateOnly from, DateOnly to)
        {
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (schedule.Weekdays.HasValue && !schedule.Weekdays.Value.Includes(date.DayOfWeek))
                    continue;

                yield return new QuestPeriodWindow(date, date);
            }
        }

        private static IEnumerable<QuestPeriodWindow> WeeklyWindows(DayOfWeek weekStartsOn, DateOnly from, DateOnly to)
        {
            for (var start = StartOfWeek(from, weekStartsOn); start <= to; start = start.AddDays(7))
                yield return new QuestPeriodWindow(start, start.AddDays(6));
        }

        private static IEnumerable<QuestPeriodWindow> MonthlyWindows(QuestSchedule schedule, DateOnly from, DateOnly to)
        {
            // Start a month early: a window like "the 28th to the 30th" can begin before `from` and still
            // overlap it.
            var month = new DateOnly(from.Year, from.Month, 1).AddMonths(-1);
            var lastMonth = new DateOnly(to.Year, to.Month, 1);

            for (; month <= lastMonth; month = month.AddMonths(1))
            {
                var window = MonthWindow(schedule, month);

                if (window.End >= from && window.Start <= to)
                    yield return window;
            }
        }

        private static QuestPeriodWindow MonthWindow(QuestSchedule schedule, DateOnly month)
        {
            int daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);

            if (schedule.MonthWindowStartDay is not int startDay)
                return new QuestPeriodWindow(month, new DateOnly(month.Year, month.Month, daysInMonth));

            // Clamp so "the 31st" still yields a period in February.
            var start = new DateOnly(month.Year, month.Month, Math.Min(startDay, daysInMonth));
            var end = new DateOnly(month.Year, month.Month, Math.Min(schedule.MonthWindowEndDay!.Value, daysInMonth));

            return new QuestPeriodWindow(start, end);
        }

        private static IEnumerable<QuestPeriodWindow> YearlyWindows(QuestSchedule schedule, DateOnly from, DateOnly to)
        {
            // A wrapping window (Dec 21 to Mar 20) belonging to the previous year can still overlap `from`.
            for (int year = from.Year - 1; year <= to.Year; year++)
            {
                var window = YearWindow(schedule, year);

                if (window.End >= from && window.Start <= to)
                    yield return window;
            }
        }

        private static QuestPeriodWindow YearWindow(QuestSchedule schedule, int year)
        {
            if (schedule.YearWindowStart is not int windowStart)
                return new QuestPeriodWindow(new DateOnly(year, 1, 1), new DateOnly(year, 12, 31));

            int windowEnd = schedule.YearWindowEnd!.Value;

            var start = MonthDayOn(windowStart, year);
            // An end before the start means the window runs past new year — a Winter season.
            var end = MonthDayOn(windowEnd, windowEnd < windowStart ? year + 1 : year);

            return new QuestPeriodWindow(start, end);
        }

        /// <summary>Resolves an MMDD into a real date, clamping February 29th in a non-leap year.</summary>
        private static DateOnly MonthDayOn(int monthDay, int year)
        {
            int month = monthDay / 100;
            int day = Math.Min(monthDay % 100, DateTime.DaysInMonth(year, month));

            return new DateOnly(year, month, day);
        }

        // ──────────────────────────────── alignment ──────────────────────────────

        /// <summary>
        /// Whether a canonical window survives the schedule's "every N units" filter, counted from the anchor.
        /// </summary>
        private static bool IsAligned(
            QuestSchedule schedule,
            QuestScheduleBounds bounds,
            DayOfWeek weekStartsOn,
            QuestPeriodWindow window)
        {
            if (schedule.Interval <= 1)
                return true;

            var anchor = bounds.Anchor;

            int unitsFromAnchor = schedule.Unit switch
            {
                PeriodUnitEnum.Day => window.Start.DayNumber - anchor.DayNumber,
                PeriodUnitEnum.Week => (window.Start.DayNumber - StartOfWeek(anchor, weekStartsOn).DayNumber) / 7,
                PeriodUnitEnum.Month => ((window.Start.Year - anchor.Year) * 12) + window.Start.Month - anchor.Month,
                PeriodUnitEnum.Year => window.Start.Year - anchor.Year,
                _ => 0
            };

            return Mod(unitsFromAnchor, schedule.Interval) == 0;
        }

        // ───────────────────────────────── clipping ──────────────────────────────

        /// <summary>
        /// Cuts a canonical window down to the quest's active range, keeping the untruncated bounds so the
        /// caller can prorate the target. Returns null when nothing of the period is inside the range.
        /// </summary>
        private static QuestPeriodWindow? Clip(QuestPeriodWindow window, QuestScheduleBounds bounds)
        {
            var start = window.Start;
            var end = window.End;

            if (start < bounds.EffectiveFrom)
                start = bounds.EffectiveFrom;

            if (bounds.ActiveTo is DateOnly activeTo && end > activeTo)
                end = activeTo;

            if (start > end)
                return null;

            return new QuestPeriodWindow(start, end, window.Start, window.End);
        }

        /// <summary>Modulo that stays non-negative for dates before the anchor, unlike <c>%</c>.</summary>
        private static int Mod(int value, int modulus) => ((value % modulus) + modulus) % modulus;
    }
}
