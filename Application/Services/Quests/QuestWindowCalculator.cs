using Domain.Common;
using Domain.Enum;
using Domain.Models;
using NodaTime;

namespace Application.Services.Quests
{
    public class QuestWindowCalculator
    {
        public static List<TimeWindow> GenerateWindows(Quest quest, DateTime fromUtc, DateTime toUtc)
        {
            var userZone = DateTimeZoneProviders.Tzdb[quest.Account.TimeZone];

            return quest.QuestType switch
            {
                QuestTypeEnum.Daily => GenerateDailyWindows(fromUtc, toUtc, userZone),
                QuestTypeEnum.Weekly => GenerateWeeklyWindows(quest, fromUtc, toUtc, userZone),
                QuestTypeEnum.Monthly => GenerateMonthlyWindows(quest, fromUtc, toUtc, userZone),
                _ => []
            };
        }

        private static List<TimeWindow> GenerateDailyWindows(DateTime fromUtc, DateTime toUtc, DateTimeZone userZone)
        {
            var windows = new List<TimeWindow>();
            var fromLocal = Instant.FromDateTimeUtc(DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc)).InZone(userZone).Date;
            var toLocal = Instant.FromDateTimeUtc(DateTime.SpecifyKind(toUtc, DateTimeKind.Utc)).InZone(userZone).Date;

            for (var date = fromLocal; date <= toLocal; date = date.PlusDays(1))
            {
                var start = date.AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();
                var end = date.PlusDays(1).AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();
                windows.Add(new TimeWindow(start, end));
            }

            return windows;
        }

        private static List<TimeWindow> GenerateWeeklyWindows(Quest quest, DateTime fromUtc, DateTime toUtc, DateTimeZone userZone)
        {
            var windows = new List<TimeWindow>();
            var scheduledWeekdays = quest.WeeklyQuest_Days.Select(d => (DayOfWeek)d.Weekday).ToHashSet();

            var fromLocal = Instant.FromDateTimeUtc(DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc)).InZone(userZone).Date;
            var toLocal = Instant.FromDateTimeUtc(DateTime.SpecifyKind(toUtc, DateTimeKind.Utc)).InZone(userZone).Date;

            for (var date = fromLocal; date <= toLocal; date = date.PlusDays(1))
            {
                if (scheduledWeekdays.Contains((DayOfWeek)date.DayOfWeek))
                {
                    var start = date.AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();
                    var end = date.PlusDays(1).AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();
                    windows.Add(new TimeWindow(start, end));
                }
            }

            return windows;
        }

        private static List<TimeWindow> GenerateMonthlyWindows(Quest quest, DateTime fromUtc, DateTime toUtc, DateTimeZone userZone)
        {
            var windows = new List<TimeWindow>();
            var startDay = quest.MonthlyQuest_Days!.StartDay;
            var endDay = quest.MonthlyQuest_Days!.EndDay;

            var fromLocal = Instant.FromDateTimeUtc(DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc)).InZone(userZone).Date;
            var toLocal = Instant.FromDateTimeUtc(DateTime.SpecifyKind(toUtc, DateTimeKind.Utc)).InZone(userZone).Date;

            var startMonth = new YearMonth(fromLocal.Year, fromLocal.Month);
            var endMonth = new YearMonth(toLocal.Year, toLocal.Month);

            for (var ym = startMonth; ym <= endMonth; ym = ym.PlusMonths(1))
            {
                var daysInMonth = ym.ToDateInterval().End.Day;
                var sDay = Math.Min(startDay, daysInMonth);
                var eDay = Math.Min(endDay, daysInMonth);

                var start = ym.OnDayOfMonth(sDay).AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();
                var end = ym.OnDayOfMonth(eDay).PlusDays(1).AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();

                if (end > start)
                {
                    windows.Add(new TimeWindow(start, end));
                }
            }

            return windows;
        }
    }
}
