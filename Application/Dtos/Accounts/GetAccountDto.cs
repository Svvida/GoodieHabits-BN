namespace Application.Dtos.Accounts
{
    public class GetAccountDto
    {
        public string? Login { get; set; }
        public required string Email { get; set; }
        public string? Nickname { get; set; }
        public string? Avatar { get; set; }
        public int CompletedQuests { get; set; }
        public int TotalQuests { get; set; }
        public int CompletedGoals { get; set; }
        public int ExpiredGoals { get; set; }
        public int TotalGoals { get; set; }
        public int Level { get; set; } // Current user Level
        public int UserXp { get; set; } // User's total accumulated XP
        public int NextLevelTotalXpRequired { get; set; } // XP required to reach next level
        public bool IsMaxLevel { get; set; } // Indicates if the user is at max level
        public string? Bio { get; set; }
        public DateTime JoinDate { get; set; }
        public ICollection<GetBadgeDto> Badges { get; set; } = [];
        public ICollection<GetAccountPreferencesDto> Preferences { get; set; } = [];
    }
}