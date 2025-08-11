using Application.Statistics.Dtos;

namespace Application.Statistics.Queries.GetUserExtendedStats
{
    public record GetUserExtendedStatsResponse(QuestExtendedStatsDto QuestStats,
                                               GoalExtendedStatsDto GoalStats,
                                               XpProgressDto XpStats);
}
