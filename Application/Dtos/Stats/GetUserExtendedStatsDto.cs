using Application.Dtos.UserProfileStats;

namespace Application.Dtos.Stats
{
    public class GetUserExtendedStatsDto
    {
        public QuestExtendedStatsDto QuestStats { get; set; } = new();
        public GoalExtendedStatsDto GoalStats { get; set; } = new();
        public XpProgressDto XpStats { get; set; } = new();
    }
}
