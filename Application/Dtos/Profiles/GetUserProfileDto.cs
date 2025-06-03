namespace Application.Dtos.Profiles
{
    public class GetUserProfileDto
    {
        public string? Nickname { get; set; }
        public string? Avatar { get; set; }
        public string? Bio { get; set; }
        public DateTime JoinDate { get; set; }
        public QuestStatsDto QuestsStats { get; set; } = new();
        public GoalStatsDto GoalsStats { get; set; } = new();
        public XpProgressDto XpProgress { get; set; } = new();
        public ICollection<GetBadgeDto> Badges { get; set; } = [];
    }
}
