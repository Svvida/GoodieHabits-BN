using Domain.Enums;
using Domain.Models;

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
            DateOnly? endDate = null)
        {
            return Quest.Create(
                title: "Daily habit",
                userProfile: profile ?? Profile(),
                questType: QuestTypeEnum.Daily,
                nowUtc: nowUtc ?? DefaultNowUtc,
                startDate: startDate,
                endDate: endDate);
        }

        public static Quest Weekly(
            IEnumerable<WeekdayEnum> weekdays,
            UserProfile? profile = null,
            DateTime? nowUtc = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null)
        {
            var quest = Quest.Create(
                title: "Weekly habit",
                userProfile: profile ?? Profile(),
                questType: QuestTypeEnum.Weekly,
                nowUtc: nowUtc ?? DefaultNowUtc,
                startDate: startDate,
                endDate: endDate);

            quest.SetWeekdays(weekdays);
            return quest;
        }

        public static Quest Monthly(
            int startDay,
            int endDay,
            UserProfile? profile = null,
            DateTime? nowUtc = null,
            DateOnly? startDate = null,
            DateOnly? endDate = null)
        {
            var quest = Quest.Create(
                title: "Monthly habit",
                userProfile: profile ?? Profile(),
                questType: QuestTypeEnum.Monthly,
                nowUtc: nowUtc ?? DefaultNowUtc,
                startDate: startDate,
                endDate: endDate);

            quest.SetMonthlyDays(startDay, endDay);
            return quest;
        }

        /// <summary>Adds a period and optionally marks it completed, bypassing the quest-level flow.</summary>
        public static QuestOccurrence AddPeriod(Quest quest, DateOnly start, DateOnly? end = null, DateTime? completedAtUtc = null)
        {
            var occurrence = quest.AddOccurrence(start, end ?? start);

            if (completedAtUtc.HasValue)
                occurrence.MarkAsCompleted(completedAtUtc.Value, start);

            return occurrence;
        }
    }
}
