using Domain.Enum;
using Domain.Exceptions;
using Domain.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Domain.Calculators
{
    public static class NextResetDateCalculator
    {
        public static DateTime? Calculate(Quest quest)
        {
            Instant nowUtc = SystemClock.Instance.GetCurrentInstant();
            DateTimeZone userTimeZone = DateTimeZoneProviders.Tzdb[quest.Account.TimeZone]
                ?? throw new InvalidArgumentException("Invalid time zone during next reset date calculation.");

            ZonedDateTime nowLocal = nowUtc.InZone(userTimeZone);

            return quest.QuestType switch
            {
                QuestTypeEnum.Daily => CalculateDaily(quest, nowLocal, userTimeZone),
                QuestTypeEnum.Weekly => CalculateWeekly(quest, nowLocal, userTimeZone),
                QuestTypeEnum.Monthly => CalculateMonthly(quest, nowLocal, userTimeZone),
                _ => null,
            };
        }

        private static DateTime? CalculateDaily(Quest quest, ZonedDateTime nowLocal, DateTimeZone userTimeZone)
        {
            LocalDateTime nextResetLocal = nowLocal.Date.PlusDays(1).AtMidnight();
            DateTime nextResetUtc = nextResetLocal.InZoneLeniently(userTimeZone).WithZone(DateTimeZone.Utc).ToDateTimeUtc();
            if (quest.EndDate.HasValue && nextResetUtc >= quest.EndDate)
                return null;
            return nextResetUtc;
        }

        private static DateTime? CalculateWeekly(Quest quest, ZonedDateTime nowLocal, DateTimeZone userTimeZone)
        {
            // Select all available days for the quest and order them by day of the week
            var availableDays = quest.WeeklyQuest_Days.Select(wqd => (DayOfWeek)wqd.Weekday).OrderBy(wd => wd).ToList();

            // Should never happen because the quest should have at least one day
            if (availableDays.Count == 0)
                return null;

            DayOfWeek currentDay = nowLocal.DayOfWeek.ToDayOfWeek();
            DayOfWeek? nextResetDay = availableDays.FirstOrDefault(wd => wd > currentDay);

            // If there is no future day, wrap around to the first available day
            if (!availableDays.Any(wd => wd > currentDay))
                nextResetDay = availableDays.First();

            // Calculate the number of days until the next reset day and make sure it is not negative number
            int daysUntilNextReset = ((int)nextResetDay - (int)currentDay + 7) % 7;
            // If the next reset day is the same as the current day, the quest will reset in 7 days
            if (daysUntilNextReset == 0)
                daysUntilNextReset = 7;

            LocalDateTime nextResetLocal = nowLocal.Date.PlusDays(daysUntilNextReset).AtMidnight();
            DateTime nextResetUtc = nextResetLocal.InZoneLeniently(userTimeZone).WithZone(DateTimeZone.Utc).ToDateTimeUtc();

            if (quest.EndDate.HasValue && nextResetUtc >= quest.EndDate)
                return null;

            return nextResetUtc;
        }

        private static DateTime? CalculateMonthly(Quest quest, ZonedDateTime nowLocal, DateTimeZone userTimeZone)
        {
            YearMonth nextResetMonth = nowLocal.Date.PlusMonths(1).ToYearMonth();

            int startDay = quest.MonthlyQuest_Days!.StartDay;
            int lastDayOfMonth = nextResetMonth.ToDateInterval().End.Day;

            if (startDay > lastDayOfMonth)
                startDay = lastDayOfMonth; // If the start day is greater than the last day of the month, set it to the last day of the month

            LocalDateTime nextResetLocal = nextResetMonth.OnDayOfMonth(startDay).AtMidnight();
            DateTime nextResetUtc = nextResetLocal.InZoneLeniently(userTimeZone).WithZone(DateTimeZone.Utc).ToDateTimeUtc();

            if (quest.EndDate.HasValue && nextResetUtc >= quest.EndDate)
                return null;

            return nextResetUtc;
        }
    }
}
