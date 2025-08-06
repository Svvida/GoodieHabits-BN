using Domain.Models;
using Mapster;

namespace Application.Statistics.GetUserProfileStats
{
    public class GetUserProfileStatsMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UserProfile, GetUserProfileStatsResponse>()
                .Map(dest => dest.QuestStats, src => src)
                .Map(dest => dest.GoalStats, src => src)
                .Map(dest => dest.XpStats, src => src);

            config.NewConfig<UserProfile, UserProfileGoalStatsDto>()
                .Map(dest => dest.CurrentTotal, src => src.ActiveGoals)
                .Map(dest => dest.Completed, src => Math.Max(src.Account.UserGoals.Count(g => !g.IsExpired && g.IsAchieved), 0))
                .Map(dest => dest.InProgress, src => Math.Max(src.ActiveGoals - src.Account.UserGoals.Count(g => !g.IsExpired && g.IsAchieved), 0));

            config.NewConfig<UserProfile, UserProfileQuestStatsDto>()
                .Map(dest => dest.CurrentTotal, src => src.ExistingQuests)
                .Map(dest => dest.Completed, src => src.CurrentlyCompletedExistingQuests)
                .Map(dest => dest.InProgress, src => Math.Max(src.ExistingQuests - src.CurrentlyCompletedExistingQuests, 0));
        }
    }
}