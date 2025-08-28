using Domain.Models;
using Mapster;

namespace Application.Statistics.Queries.GetUserExtendedStats
{
    public class GetUserExtendedStatsMappingProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<UserProfile, GetUserExtendedStatsResponse>()
                .Map(dest => dest.QuestStats, src => src)
                .Map(dest => dest.GoalStats, src => src)
                .Map(dest => dest.XpStats, src => src);

            config.NewConfig<UserProfile, GoalExtendedStatsDto>()
                .Map(dest => dest.TotalCompleted, src => src.CompletedGoals)
                .Map(dest => dest.TotalCreated, src => src.TotalGoals)
                .Map(dest => dest.TotalExpired, src => src.ExpiredGoals)
                .Map(dest => dest.CurrentTotal, src => src.ActiveGoals)
                .Map(dest => dest.CurrentCompleted, src => Math.Max(src.Account.UserGoals.Count(g => !g.IsExpired && g.IsAchieved), 0))
                .Map(dest => dest.InProgress, src => Math.Max(src.ActiveGoals - src.Account.UserGoals.Count(g => !g.IsExpired && g.IsAchieved), 0));

            config.NewConfig<UserProfile, QuestExtendedStatsDto>()
                .Map(dest => dest.CurrentTotal, src => src.ExistingQuests)
                .Map(dest => dest.CurrentEverCompleted, src => src.EverCompletedExistingQuests)
                .Map(dest => dest.CurrentCompleted, src => src.CurrentlyCompletedExistingQuests)
                .Map(dest => dest.TotalCreated, src => src.TotalQuests)
                .Map(dest => dest.TotalCompleted, src => src.CompletedQuests)
                .Map(dest => dest.CompletedDaily, src => src.CompletedDailyQuests)
                .Map(dest => dest.CompletedWeekly, src => src.CompletedWeeklyQuests)
                .Map(dest => dest.CompletedMonthly, src => src.CompletedMonthlyQuests);
        }
    }
}