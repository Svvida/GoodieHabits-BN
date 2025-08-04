using Application.Statistics.Dtos;

namespace Application.Statistics.GetUserExtendedStats
{
    public record GetUserExtendedStatsResponse(QuestExtendedStatsDto QuestStats,
                                               GoalExtendedStatsDto GoalStats,
                                               XpProgressDto XpStats);
}
