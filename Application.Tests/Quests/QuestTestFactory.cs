using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;

namespace Application.Tests.Quests
{
    /// <summary>
    /// Builds quests through the real domain factories (no reflection, no private-setter pokes), so these
    /// tests exercise the same invariants production code does.
    /// </summary>
    internal static class QuestTestFactory
    {
        public static readonly DateTime DefaultNowUtc = new(2020, 8, 1, 12, 0, 0, DateTimeKind.Utc);

        public static UserProfile Profile(string timeZone = "Europe/Warsaw")
        {
            return Account.Create("hash", "tester@example.com", "tester", timeZone).Profile;
        }

        public static Quest Daily(
            UserProfile? profile = null,
            DateTime? nowUtc = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            decimal target = 1m,
            int interval = 1)
        {
            return Build("Daily habit", QuestSchedule.Daily(interval), QuestTarget.Create(target),
                profile, nowUtc, startDate, endDate);
        }

        /// <summary>The old Weekly quest: due on named weekdays, so a Day schedule with a weekday filter.</summary>
        public static Quest OnWeekdays(
            IEnumerable<WeekdayEnum> weekdays,
            UserProfile? profile = null,
            DateTime? nowUtc = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null)
        {
            return Build("Weekday habit", QuestSchedule.Daily(weekdays: weekdays.ToFlags()), QuestTarget.Once(),
                profile, nowUtc, startDate, endDate);
        }

        /// <summary>"N times a week, any days" — the shape the old model could not express at all.</summary>
        public static Quest TimesPerWeek(
            decimal target,
            UserProfile? profile = null,
            DateTime? nowUtc = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            int? maxPerDay = 1)
        {
            return Build("Weekly target habit", QuestSchedule.Weekly(),
                QuestTarget.Create(target, maxCompletionsPerDay: maxPerDay),
                profile, nowUtc, startDate, endDate);
        }

        public static Quest Monthly(
            int startDay,
            int endDay,
            UserProfile? profile = null,
            DateTime? nowUtc = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null)
        {
            return Build("Monthly habit", QuestSchedule.Monthly(windowStartDay: startDay, windowEndDay: endDay),
                QuestTarget.Once(), profile, nowUtc, startDate, endDate);
        }

        public static Quest OneTime(
            UserProfile? profile = null,
            DateTime? nowUtc = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            decimal target = 1m)
        {
            return Build("One-time quest", QuestSchedule.OneTime(), QuestTarget.Create(target),
                profile, nowUtc, startDate, endDate);
        }

        private static Quest Build(
            string title,
            QuestSchedule schedule,
            QuestTarget target,
            UserProfile? profile,
            DateTime? nowUtc,
            DateOnly? startDate,
            DateOnly? endDate)
        {
            return Quest.Create(
                title: title,
                userProfile: profile ?? Profile(),
                schedule: schedule,
                target: target,
                nowUtc: nowUtc ?? DefaultNowUtc,
                startDate: startDate,
                endDate: endDate);
        }

        /// <summary>Adds a period directly, optionally already satisfied, bypassing the completion flow.</summary>
        public static QuestOccurrence AddPeriod(
            Quest quest,
            DateOnly start,
            DateOnly? end = null,
            DateTime? completedAtUtc = null,
            decimal target = 1m,
            decimal progress = 0m)
        {
            var period = QuestOccurrence.Create(quest, start, end ?? start, target);
            quest.QuestOccurrences.Add(period);

            if (completedAtUtc.HasValue)
                period.ApplyProgress(target, completedAtUtc.Value, start);
            else if (progress > 0m)
                period.ApplyProgress(progress, DefaultNowUtc, start);

            return period;
        }
    }
}
