using Domain.Common;

namespace Domain.Models
{
    public class UserProfile : EntityBase
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string? Nickname { get; set; }
        public string? Avatar { get; set; }
        public int UserLevel { get; set; } = 1;
        public int TotalExperience { get; set; } = 0;
        public int CurrentExperience { get; set; } = 0;
        public int CompletedQuests { get; set; } = 0;
        public int TotalQuests { get; set; } = 0;
        public string? Bio { get; set; }

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
