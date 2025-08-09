using Domain.Enum;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Extensions;

namespace Application.Services.Quests
{
    public class QuestResetService(
        ILogger<QuestResetService> logger,
        IClock clock) : IQuestResetService
    {
        public DateTime? GetNextResetTimeUtc(Quest quest)
        {
            logger.LogDebug("Calculating NextResetAt for Quest ID: {QuestId}, Type: {QuestType}", quest.Id, quest.QuestType);

            Instant nowUtc = clock.GetCurrentInstant();
            DateTimeZone userTimeZone = DateTimeZoneProviders.Tzdb[quest.Account.TimeZone];

            ZonedDateTime nowLocal = nowUtc.InZone(userTimeZone);

            logger.LogDebug("Quest ID: {QuestId} - LastCompletedAt (UTC): {LastCompletedAt}, LocalTime ({TimeZone}): {LocalTime}",
                quest.Id, nowUtc, quest.Account.TimeZone, nowLocal);

            if (quest.QuestType == QuestTypeEnum.Daily)
            {
                LocalDateTime nextResetLocal = nowLocal.Date.PlusDays(1).AtMidnight();
                DateTime nextResetUtc = nextResetLocal.InZoneLeniently(userTimeZone).WithZone(DateTimeZone.Utc).ToDateTimeUtc();
                if (quest.EndDate.HasValue && nextResetUtc >= quest.EndDate)
                    return null;
                return nextResetUtc;
            }

            if (quest.QuestType == QuestTypeEnum.Weekly)
            {
                // Select all available days for the quest and order them by day of the week
                var availableDays = quest.WeeklyQuest_Days.Select(wqd => (DayOfWeek)wqd.Weekday).OrderBy(wd => wd).ToList();
                logger.LogDebug("Available days: {@availableDays}", availableDays);

                // Should never happen because the quest should have at least one day
                if (availableDays.Count == 0)
                {
                    logger.LogWarning("Quest ID: {QuestId} has no assigned weekdays. Returning null.", quest.Id);
                    return null;
                }

                DayOfWeek currentDay = nowLocal.DayOfWeek.ToDayOfWeek();
                DayOfWeek? nextResetDay = availableDays.FirstOrDefault(wd => wd > currentDay);

                // If there is no future day, wrap around to the first available day
                if (!availableDays.Any(wd => wd > currentDay))
                    nextResetDay = availableDays.First();

                logger.LogDebug("Current day: {currentDay}, Next reset day: {nextResetDay}", currentDay, nextResetDay);

                // Calculate the number of days until the next reset day and make sure it is not negative number
                int daysUntilNextReset = ((int)nextResetDay - (int)currentDay + 7) % 7;
                logger.LogDebug("Days until next Reset: {daysUntilNextReset}", daysUntilNextReset);
                // If the next reset day is the same as the current day, the quest will reset in 7 days
                if (daysUntilNextReset == 0)
                    daysUntilNextReset = 7;

                LocalDateTime nextResetLocal = nowLocal.Date.PlusDays(daysUntilNextReset).AtMidnight();
                DateTime nextResetUtc = nextResetLocal.InZoneLeniently(userTimeZone).WithZone(DateTimeZone.Utc).ToDateTimeUtc();

                logger.LogDebug("Quest ID: {QuestId} - Next reset day: {NextResetDay}, UTC Time: {NextResetUtc}",
                    quest.Id, nextResetDay, nextResetUtc);

                if (quest.EndDate.HasValue && nextResetUtc >= quest.EndDate)
                    return null;

                return nextResetUtc;
            }

            if (quest.QuestType == QuestTypeEnum.Monthly)
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

            return null;
        }
    }
}
