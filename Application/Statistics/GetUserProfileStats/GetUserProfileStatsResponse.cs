using Application.Statistics.Dtos;

namespace Application.Statistics.GetUserProfileStats
{
    public record GetUserProfileStatsResponse(UserProfileQuestStatsDto QuestStats,
                                             UserProfileGoalStatsDto GoalStats,
                                             XpProgressDto XpStats);
}
