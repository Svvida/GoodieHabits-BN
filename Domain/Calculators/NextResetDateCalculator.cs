using Domain.Enums;
using Domain.Exceptions;
using Domain.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Domain.Calculators
{
    /// <summary>
    /// Works out when a repeatable quest's completed flag should next be cleared.
    /// <para>
    /// Unlike occurrence periods, this genuinely *is* an instant — it is the moment the background
    /// reset job should act — so it stays a UTC <see cref="DateTime"/>. The date arithmetic is done on
    /// the user's local calendar and converted to UTC exactly once, at the end.
    /// </para>
    /// </summary>
    public static class NextResetDateCalculator
    {
        public static DateTime? Calculate(Quest quest, DateTime nowUtc)
        {
            DateTimeZone userTimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(quest.UserProfile.TimeZone)
                ?? throw new InvalidArgumentException("Invalid time zone during next reset date calculation.");

            LocalDate todayLocal = Instant.FromDateTimeUtc(DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc))
                .InZone(userTimeZone)
                .Date;

            LocalDate? nextResetLocal = quest.QuestType switch
            {
                QuestTypeEnum.Daily => todayLocal.PlusDays(1),
                QuestTypeEnum.Weekly => CalculateWeekly(quest, todayLocal),
                QuestTypeEnum.Monthly => CalculateMonthly(quest, todayLocal),
                _ => null,
            };

            if (nextResetLocal is null)
                return null;

            var nextResetDate = new DateOnly(nextResetLocal.Value.Year, nextResetLocal.Value.Month, nextResetLocal.Value.Day);
            if (quest.EndDate.HasValue && nextResetDate > quest.EndDate.Value)
                return null;

            return nextResetLocal.Value.AtMidnight().InZoneLeniently(userTimeZone).ToDateTimeUtc();
        }

        private static LocalDate? CalculateWeekly(Quest quest, LocalDate todayLocal)
        {
            // Ordered on the same numbering as System.DayOfWeek (Sunday = 0).
            var availableDays = quest.WeeklyQuest_Days
                .Select(wqd => (int)wqd.Weekday)
                .OrderBy(wd => wd)
                .ToList();

            // Should never happen — a weekly quest always has at least one day.
            if (availableDays.Count == 0)
                return null;

            int currentDay = (int)todayLocal.DayOfWeek.ToDayOfWeek();

            int daysUntilNextReset = availableDays
                .Select(day => (day - currentDay + 7) % 7)
                .Where(offset => offset > 0)
                .DefaultIfEmpty(7)
                .Min();

            return todayLocal.PlusDays(daysUntilNextReset);
        }

        private static LocalDate CalculateMonthly(Quest quest, LocalDate todayLocal)
        {
            var nextResetMonth = todayLocal.PlusMonths(1);

            int startDay = quest.MonthlyQuest_Days!.StartDay;
            int lastDayOfMonth = CalendarSystem.Iso.GetDaysInMonth(nextResetMonth.Year, nextResetMonth.Month);

            // Clamp so e.g. "the 31st" still resets in a short month.
            if (startDay > lastDayOfMonth)
                startDay = lastDayOfMonth;

            return new LocalDate(nextResetMonth.Year, nextResetMonth.Month, startDay);
        }
    }
}
