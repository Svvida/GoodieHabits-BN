using Domain.Enums;
using Domain.Models;

namespace Tests.Factories
{
    public static class QuestFactory
    {
        public static Quest CreateDailyQuest(int accountId, DateTime? lastCompletedAt = null, string timeZone = "Etc/UTC")
        {
            return new Quest
            {
                Id = 1,
                AccountId = accountId,
                QuestType = QuestTypeEnum.Daily,
                Title = "Test Daily Quest",
                LastCompletedAt = lastCompletedAt ?? DateTime.UtcNow,
                Account = AccountFactory.CreateAccount(accountId, "hash", "email", timeZone)
            };
        }

        public static Quest CreateWeeklyQuest(int accountId, WeekdayEnum resetDay, DateTime? lastCompletedAt = null, string timeZone = "Etc/UTC")
        {
            return new Quest
            {
                Id = 2,
                AccountId = accountId,
                QuestType = QuestTypeEnum.Weekly,
                Title = "Test Weekly Quest",
                LastCompletedAt = lastCompletedAt ?? DateTime.UtcNow,
                WeeklyQuest_Days = [new WeeklyQuest_Day { Weekday = resetDay }],
                Account = AccountFactory.CreateAccount(accountId, "hash", "email", timeZone)
            };
        }

        public static Quest CreateMonthlyQuest(int accountId, int startDay, int endDay, DateTime? lastCompletedAt = null, string timeZone = "Etc/Utc")
        {
            return new Quest
            {
                Id = 3,
                AccountId = accountId,
                QuestType = QuestTypeEnum.Monthly,
                Title = "Test Monthly Quest",
                LastCompletedAt = lastCompletedAt ?? DateTime.UtcNow,
                MonthlyQuest_Days = new MonthlyQuest_Days { StartDay = startDay, EndDay = endDay },
                Account = AccountFactory.CreateAccount(accountId, "hash", "email", timeZone)
            };
        }
    }
}
