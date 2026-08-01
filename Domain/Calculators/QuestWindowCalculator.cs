using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Calculators
{
    /// <summary>
    /// Generates the local calendar periods a repeatable quest should own occurrences for.
    /// <para>
    /// This is pure calendar arithmetic — no timezone conversion happens here. The caller has already
    /// resolved "which local day is it for this user" via <see cref="UserProfile.LocalDateOn"/>, which
    /// is what makes occurrence periods stable when the user's timezone changes.
    /// </para>
    /// </summary>
    public static class QuestWindowCalculator
    {
        public static IReadOnlyList<QuestOccurrenceWindow> GenerateWindows(Quest quest, DateOnly fromDate, DateOnly toDate)
        {
            if (toDate < fromDate)
                return [];

            return quest.QuestType switch
            {
                QuestTypeEnum.Daily => GenerateDailyWindows(fromDate, toDate),
                QuestTypeEnum.Weekly => GenerateWeeklyWindows(quest, fromDate, toDate),
                QuestTypeEnum.Monthly => GenerateMonthlyWindows(quest, fromDate, toDate),
                _ => []
            };
        }

        private static IReadOnlyList<QuestOccurrenceWindow> GenerateDailyWindows(DateOnly fromDate, DateOnly toDate)
        {
            var windows = new List<QuestOccurrenceWindow>();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                windows.Add(new QuestOccurrenceWindow(date, date));

            return windows;
        }

        private static IReadOnlyList<QuestOccurrenceWindow> GenerateWeeklyWindows(Quest quest, DateOnly fromDate, DateOnly toDate)
        {
            var scheduledWeekdays = quest.WeeklyQuest_Days.Select(d => d.Weekday).ToHashSet();
            var windows = new List<QuestOccurrenceWindow>();

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (scheduledWeekdays.Contains((WeekdayEnum)date.DayOfWeek))
                    windows.Add(new QuestOccurrenceWindow(date, date));
            }

            return windows;
        }

        private static IReadOnlyList<QuestOccurrenceWindow> GenerateMonthlyWindows(Quest quest, DateOnly fromDate, DateOnly toDate)
        {
            var startDay = quest.MonthlyQuest_Days!.StartDay;
            var endDay = quest.MonthlyQuest_Days!.EndDay;

            var windows = new List<QuestOccurrenceWindow>();
            var lastMonth = new DateOnly(toDate.Year, toDate.Month, 1);

            for (var month = new DateOnly(fromDate.Year, fromDate.Month, 1); month <= lastMonth; month = month.AddMonths(1))
            {
                // Clamp to the real length of the month so e.g. "the 31st" still yields a period in February.
                var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
                var start = new DateOnly(month.Year, month.Month, Math.Min(startDay, daysInMonth));
                var end = new DateOnly(month.Year, month.Month, Math.Min(endDay, daysInMonth));

                if (end >= start)
                    windows.Add(new QuestOccurrenceWindow(start, end));
            }

            return windows;
        }
    }
}
