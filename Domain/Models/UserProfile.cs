using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;
using NodaTime;

namespace Domain.Models
{
    public class UserProfile : EntityBase
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string TimeZone { get; private set; } = "Etc/UTC";
        public string? Nickname { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public int TotalXp { get; set; } = 0;
        // Stats for quests
        public int CompletedQuests { get; set; } = 0;
        public int CompletedDailyQuests { get; set; } = 0;
        public int CompletedWeeklyQuests { get; set; } = 0;
        public int CompletedMonthlyQuests { get; set; } = 0;
        public int TotalQuests { get; set; } = 0;
        public int ExistingQuests { get; set; } = 0;
        public int CurrentlyCompletedExistingQuests { get; set; } = 0;
        public int EverCompletedExistingQuests { get; set; } = 0;
        // Stats for goals
        public int CompletedGoals { get; set; } = 0;
        public int ExpiredGoals { get; set; } = 0;
        public int TotalGoals { get; set; } = 0;
        public int ActiveGoals { get; set; } = 0;

        public Account Account { get; set; } = null!;
        public ICollection<Quest> Quests { get; set; } = [];
        public ICollection<QuestLabel> Labels { get; set; } = [];
        public ICollection<UserGoal> UserGoals { get; set; } = [];
        public ICollection<UserProfile_Badge> UserProfile_Badges { get; set; } = [];
        public ICollection<Notification> Notifications { get; set; } = [];

        public UserProfile() { }
        public UserProfile(Account account, string timeZone = "Etc/Utc")
        {
            Account = account ?? throw new InvalidArgumentException("Account cannot be null.");
            TimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZone)?.Id
                ?? throw new Exceptions.InvalidTimeZoneException(Id, timeZone);
        }

        public void UpdateTimeZone(string? timeZone)
        {
            if (string.IsNullOrWhiteSpace(timeZone))
                throw new InvalidArgumentException("TimeZone cannot be null or whitespace.");
            TimeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZone)?.Id
                ?? throw new Exceptions.InvalidTimeZoneException(Id, timeZone);
        }

        public void WipeoutData()
        {
            Avatar = null;
            Bio = null;
            TotalXp = 0;
            CompletedQuests = 0;
            TotalQuests = 0;
            ExistingQuests = 0;
            CurrentlyCompletedExistingQuests = 0;
            EverCompletedExistingQuests = 0;
            CompletedGoals = 0;
            ExpiredGoals = 0;
            TotalGoals = 0;
            ActiveGoals = 0;

            UserProfile_Badges.Clear();
        }

        public void ApplyQuestCompletionRewards(int xpAwarded, bool isGoalCompleted, bool isFirstTimeCompleted, bool shouldAssignRewards, QuestTypeEnum questType)
        {
            if (shouldAssignRewards)
            {
                CompletedQuests++;
                TotalXp += xpAwarded;
            }
            if (isFirstTimeCompleted)
                EverCompletedExistingQuests++;
            if (isGoalCompleted)
                CompletedGoals++;
            CurrentlyCompletedExistingQuests++;

            switch (questType)
            {
                case QuestTypeEnum.Daily:
                    CompletedDailyQuests++;
                    break;
                case QuestTypeEnum.Weekly:
                    CompletedWeeklyQuests++;
                    break;
                case QuestTypeEnum.Monthly:
                    CompletedMonthlyQuests++;
                    break;
            }
        }

        public void RevertQuestCompletion(QuestTypeEnum questType)
        {
            CurrentlyCompletedExistingQuests = Math.Max(CurrentlyCompletedExistingQuests - 1, 0);
            switch (questType)
            {
                case QuestTypeEnum.Daily:
                    CompletedDailyQuests = Math.Max(CompletedDailyQuests - 1, 0);
                    break;
                case QuestTypeEnum.Weekly:
                    CompletedWeeklyQuests = Math.Max(CompletedWeeklyQuests - 1, 0);
                    break;
                case QuestTypeEnum.Monthly:
                    CompletedMonthlyQuests = Math.Max(CompletedMonthlyQuests - 1, 0);
                    break;
            }
        }

        public void UpdateAfterQuestDeletion(
            bool isQuestCompleted,
            bool isQuestEverCompleted,
            bool isQuestActiveGoal)
        {
            ExistingQuests = Math.Max(ExistingQuests - 1, 0);
            if (isQuestCompleted)
                CurrentlyCompletedExistingQuests = Math.Max(CurrentlyCompletedExistingQuests - 1, 0);
            if (isQuestEverCompleted)
                EverCompletedExistingQuests = Math.Max(EverCompletedExistingQuests - 1, 0);
            if (isQuestActiveGoal)
                ActiveGoals = Math.Max(ActiveGoals - 1, 0);
        }

        public void UpdateAfterQuestCreation()
        {
            ExistingQuests++;
            TotalQuests++;
        }

        public void UpdateAfterUserGoalCreation()
        {
            ActiveGoals++;
            TotalGoals++;
        }

        public void UpdateNickname(string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
                throw new InvalidArgumentException("Nickname cannot be null or whitespace.");
            Nickname = nickname;
        }

        public void UpdateBio(string? bio)
        {
            if (bio != null && bio.Length > 30)
                throw new InvalidArgumentException("Bio cannot exceed 30 characters.");
            Bio = bio;
        }

        public void IncrementExpiredGoals(int count)
        {
            if (count <= 0)
                return;

            ExpiredGoals += count;
            ActiveGoals = Math.Max(ActiveGoals - count, 0);
        }

        public void DecrementCompletedQuestsAfterReset(int count)
        {
            if (count <= 0)
                return;

            CurrentlyCompletedExistingQuests = Math.Max(CurrentlyCompletedExistingQuests - count, 0);
        }

        public int ExpireGoals(DateTime nowUtc)
        {
            int expiredCount = 0;
            foreach (var goal in UserGoals)
            {
                if (goal.Expire(nowUtc))
                    expiredCount++;
            }
            IncrementExpiredGoals(expiredCount);
            return expiredCount;
        }

        public int ResetQuests(DateTime nowUtc)
        {
            int resetCount = 0;
            foreach (var quest in Quests)
            {
                if (quest.ResetCompletedStatus(nowUtc))
                    resetCount++;
            }
            DecrementCompletedQuestsAfterReset(resetCount);
            return resetCount;
        }
    }
}
