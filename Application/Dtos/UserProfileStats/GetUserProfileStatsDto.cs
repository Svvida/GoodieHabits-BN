namespace Application.Dtos.UserProfileStats
{
    public class GetUserProfileStatsDto
    {
        public QuestStatsDto Quests { get; set; } = new();
        public GoalStatsDto Goals { get; set; } = new();
        public XpProgressDto XpProgress { get; set; } = new();

    }
}
