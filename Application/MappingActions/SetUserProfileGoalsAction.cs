using Application.Dtos.UserProfileStats;
using AutoMapper;
using Domain.Models;

namespace Application.MappingActions
{
    internal class SetUserProfileGoalsAction : IMappingAction<UserProfile, GoalStatsDto>
    {
        public void Process(UserProfile source, GoalStatsDto destination, ResolutionContext context)
        {
            var goals = source.Account.UserGoals;

            destination.CurrentTotal = source.ActiveGoals;
            destination.Completed = goals.Count(g => !g.IsExpired && g.IsAchieved);
            destination.InProgress = Math.Max(source.ActiveGoals - destination.Completed, 0);
        }
    }
}
