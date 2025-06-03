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
        public int CompletedExistingQuests { get; set; } = 0;
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
    }
}
