using Application.Statistics.Dtos;

namespace Application.Statistics.Queries.GetUserProfileStats
{
    public record GetUserProfileStatsResponse(UserProfileQuestStatsDto QuestStats,
                                             UserProfileGoalStatsDto GoalStats,
                                             XpProgressDto XpStats);
}
