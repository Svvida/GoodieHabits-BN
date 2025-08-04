using AutoMapper;
using Domain.Models;

namespace Application.Statistics.GetUserProfileStats
{
    public class GetUserProfileGoalsResolver : IValueResolver<UserProfile, GetUserProfileStatsResponse, UserProfileGoalStatsDto>
    {
        public UserProfileGoalStatsDto Resolve(UserProfile source, GetUserProfileStatsResponse destination, UserProfileGoalStatsDto member, ResolutionContext context)
        {
            var goals = source.Account.UserGoals;

            member.CurrentTotal = source.ActiveGoals;
            member.Completed = goals.Count(g => !g.IsExpired && g.IsAchieved);
            member.InProgress = Math.Max(source.ActiveGoals - member.Completed, 0);

            return member;
        }
    }
}
