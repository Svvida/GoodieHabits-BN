namespace Domain.Enums
{
    /// <summary>
    /// A set of weekdays in one int column, replacing the <c>WeeklyQuest_Day</c> satellite table.
    /// Values mirror <see cref="WeekdayEnum"/> (and therefore <see cref="System.DayOfWeek"/>) so the two
    /// convert by shifting: <c>1 &lt;&lt; (int)weekday</c>.
    /// </summary>
    [Flags]
    public enum WeekdayFlags
    {
        None = 0,
        Sunday = 1 << 0,
        Monday = 1 << 1,
        Tuesday = 1 << 2,
        Wednesday = 1 << 3,
        Thursday = 1 << 4,
        Friday = 1 << 5,
        Saturday = 1 << 6,

        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,
        Weekend = Saturday | Sunday,
        All = Weekdays | Weekend
    }

    public static class WeekdayFlagsExtensions
    {
        public static WeekdayFlags ToFlag(this DayOfWeek day) => (WeekdayFlags)(1 << (int)day);

        public static WeekdayFlags ToFlag(this WeekdayEnum day) => (WeekdayFlags)(1 << (int)day);

        public static bool Includes(this WeekdayFlags flags, DayOfWeek day) => (flags & day.ToFlag()) != 0;

        public static IEnumerable<WeekdayEnum> ToWeekdays(this WeekdayFlags flags)
        {
            for (int i = 0; i <= 6; i++)
            {
                if ((flags & (WeekdayFlags)(1 << i)) != 0)
                    yield return (WeekdayEnum)i;
            }
        }

        public static WeekdayFlags ToFlags(this IEnumerable<WeekdayEnum> days)
        {
            var result = WeekdayFlags.None;
            foreach (var day in days)
                result |= day.ToFlag();

            return result;
        }
    }
}
