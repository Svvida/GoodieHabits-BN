using Domain.Common;

namespace Domain.Models
{
    public class UserProfile : EntityBase
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string? Nickname { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public int TotalXp { get; set; } = 0;
        // Stats for quests
        public int CompletedQuests { get; set; } = 0;
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
        public ICollection<UserProfile_Badge> UserProfile_Badges { get; set; } = [];

        public UserProfile() { }
        public UserProfile(int id, int accountId)
        {
            Id = id;
            AccountId = accountId;
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

        public void ApplyQuestCompletionRewards(int xpAwarded, int goalsCompleted, bool isFirstTimeCompleted, bool shouldAssignRewards)
        {
            if (shouldAssignRewards)
            {
                CompletedQuests++;
                TotalXp += xpAwarded;
            }
            if (isFirstTimeCompleted)
                EverCompletedExistingQuests++;
            CurrentlyCompletedExistingQuests++;
            CompletedGoals += goalsCompleted;
        }

        public void RevertQuestCompletion()
        {
            CurrentlyCompletedExistingQuests = Math.Max(CurrentlyCompletedExistingQuests - 1, 0);
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
    }
}
