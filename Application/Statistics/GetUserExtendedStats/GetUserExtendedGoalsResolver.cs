using AutoMapper;
using Domain.Models;

namespace Application.Statistics.GetUserExtendedStats
{
    public class GetUserExtendedGoalsResolver : IValueResolver<UserProfile, GetUserExtendedStatsResponse, GoalExtendedStatsDto>
    {
        public GoalExtendedStatsDto Resolve(UserProfile source, GetUserExtendedStatsResponse destination, GoalExtendedStatsDto member, ResolutionContext context)
        {
            var goals = source.Account.UserGoals;

            member.CurrentTotal = source.ActiveGoals;
            member.CurrentCompleted = goals.Count(g => !g.IsExpired && g.IsAchieved);
            member.InProgress = source.ActiveGoals - member.CurrentCompleted;
            member.TotalCreated = source.TotalGoals;
            member.TotalCompleted = source.CompletedGoals;
            member.TotalExpired = source.ExpiredGoals;

            return member;
        }
    }
}
