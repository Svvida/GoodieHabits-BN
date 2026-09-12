using Domain.Enums;
using Domain.Exceptions;

namespace Domain.ValueObjects
{
    /// <summary>
    /// Which calendar periods a quest owns — the "when", with no notion of how much counts as done
    /// (that is <see cref="QuestTarget"/>). Replaces <c>QuestTypeEnum</c> plus the
    /// <c>WeeklyQuest_Day</c> / <c>MonthlyQuest_Days</c> / <c>SeasonalQuest_Season</c> satellite tables.
    /// <para>
    /// Deliberately structured columns rather than a stored RRULE string: they stay queryable and
    /// validatable, and converting to RRULE for a calendar export is a pure function over them.
    /// </para>
    /// </summary>
    public sealed record QuestSchedule
    {
        public const int MaxInterval = 366;

        public PeriodUnitEnum Unit { get; }

        /// <summary>Every N units. Anchored on the quest's start date, so "every other day" is stable.</summary>
        public int Interval { get; }

        /// <summary><see cref="PeriodUnitEnum.Day"/> only: which weekdays are periods. Null means every day.</summary>
        public WeekdayFlags? Weekdays { get; }

        /// <summary><see cref="PeriodUnitEnum.Month"/> only: 1..31, clamped to each month's real length.</summary>
        public int? MonthWindowStartDay { get; }
        public int? MonthWindowEndDay { get; }

        /// <summary>
        /// <see cref="PeriodUnitEnum.Year"/> only: the window as MMDD (December 21st is <c>1221</c>).
        /// A window whose end is before its start wraps across new year — which is how a Winter season
        /// is expressed.
        /// </summary>
        public int? YearWindowStart { get; }
        public int? YearWindowEnd { get; }

        private QuestSchedule(
            PeriodUnitEnum unit,
            int interval,
            WeekdayFlags? weekdays,
            int? monthWindowStartDay,
            int? monthWindowEndDay,
            int? yearWindowStart,
            int? yearWindowEnd)
        {
            Unit = unit;
            Interval = interval;
            Weekdays = weekdays;
            MonthWindowStartDay = monthWindowStartDay;
            MonthWindowEndDay = monthWindowEndDay;
            YearWindowStart = yearWindowStart;
            YearWindowEnd = yearWindowEnd;
        }

        /// <summary>A quest with no recurrence: one period covering its whole active range.</summary>
        public static QuestSchedule OneTime() =>
            new(PeriodUnitEnum.None, 1, null, null, null, null, null);

        public static QuestSchedule Daily(int interval = 1, WeekdayFlags? weekdays = null)
        {
            ValidateInterval(interval);

            if (weekdays == WeekdayFlags.None)
                throw new InvalidArgumentException("At least one weekday must be selected.");

            if (weekdays.HasValue && (weekdays.Value & ~WeekdayFlags.All) != 0)
                throw new InvalidArgumentException("Weekdays contains a value that is not a weekday.");

            // "Every day" and "all seven weekdays" are the same schedule; normalise so they compare equal.
            if (weekdays == WeekdayFlags.All)
                weekdays = null;

            return new QuestSchedule(PeriodUnitEnum.Day, interval, weekdays, null, null, null, null);
        }

        public static QuestSchedule Weekly(int interval = 1)
        {
            ValidateInterval(interval);
            return new QuestSchedule(PeriodUnitEnum.Week, interval, null, null, null, null, null);
        }

        public static QuestSchedule Monthly(int interval = 1, int? windowStartDay = null, int? windowEndDay = null)
        {
            ValidateInterval(interval);

            if (windowStartDay.HasValue != windowEndDay.HasValue)
                throw new InvalidArgumentException("A monthly window needs both a start day and an end day.");

            if (windowStartDay.HasValue)
            {
                if (windowStartDay < 1 || windowStartDay > 31)
                    throw new InvalidArgumentException("Monthly window start day must be between 1 and 31.");
                if (windowEndDay < 1 || windowEndDay > 31)
                    throw new InvalidArgumentException("Monthly window end day must be between 1 and 31.");
                if (windowEndDay < windowStartDay)
                    throw new InvalidArgumentException("Monthly window end day cannot be before its start day.");
            }

            return new QuestSchedule(PeriodUnitEnum.Month, interval, null, windowStartDay, windowEndDay, null, null);
        }

        public static QuestSchedule Yearly(int interval = 1, int? windowStart = null, int? windowEnd = null)
        {
            ValidateInterval(interval);

            if (windowStart.HasValue != windowEnd.HasValue)
                throw new InvalidArgumentException("A yearly window needs both a start and an end.");

            if (windowStart.HasValue)
            {
                ValidateMonthDay(windowStart.Value, nameof(windowStart));
                ValidateMonthDay(windowEnd!.Value, nameof(windowEnd));
            }

            return new QuestSchedule(PeriodUnitEnum.Year, interval, null, null, null, windowStart, windowEnd);
        }

        /// <summary>True when the schedule produces a recurring series rather than one standalone period.</summary>
        public bool IsRepeatable => Unit != PeriodUnitEnum.None;

        /// <summary>
        /// True when a period can span more than one calendar day, which is what makes a per-weekday
        /// breakdown meaningless for it.
        /// </summary>
        public bool HasMultiDayPeriods => Unit is not (PeriodUnitEnum.Day);

        private static void ValidateInterval(int interval)
        {
            if (interval < 1 || interval > MaxInterval)
                throw new InvalidArgumentException($"Interval must be between 1 and {MaxInterval}.");
        }

        private static void ValidateMonthDay(int value, string name)
        {
            int month = value / 100;
            int day = value % 100;

            if (month < 1 || month > 12)
                throw new InvalidArgumentException($"{name} must encode a month between 1 and 12 (MMDD).");

            // Validated against a leap year so 0229 stays legal; generation clamps to each real year.
            if (day < 1 || day > DateTime.DaysInMonth(2024, month))
                throw new InvalidArgumentException($"{name} must encode a valid day of its month (MMDD).");
        }
    }
}
