using Application.Dtos.UserProfileStats;
using AutoMapper;
using Domain.Models;

namespace Application.Helpers
{
    public class ProfileGoalsResolver : IValueResolver<UserProfile, GetUserProfileStatsDto, GoalStatsDto>
    {
        public GoalStatsDto Resolve(UserProfile source, GetUserProfileStatsDto destination, GoalStatsDto member, ResolutionContext context)
        {
            var goals = source.Account.UserGoals;

            member.CurrentTotal = source.ActiveGoals;
            member.Completed = goals.Count(g => !g.IsExpired && g.IsAchieved);
            member.InProgress = Math.Max(source.ActiveGoals - member.Completed, 0);

            return member;
        }
    }
}
