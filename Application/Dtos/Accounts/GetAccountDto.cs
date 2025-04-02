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
        public int Level { get; set; }
        public int Xp { get; set; }
        public int TotalXP { get; set; }
        public string? Bio { get; set; }
        public DateTime JoinDate { get; set; }
        public ICollection<GetBadgeDto> Badges { get; set; } = [];
        public ICollection<GetAccountPreferencesDto> Preferences { get; set; } = [];
    }
}