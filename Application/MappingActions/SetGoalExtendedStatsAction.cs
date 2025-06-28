using Application.Dtos.Stats;
using AutoMapper;
using Domain.Models;

namespace Application.MappingActions
{
    internal class SetGoalExtendedStatsAction : IMappingAction<UserProfile, GoalExtendedStatsDto>
    {
        public void Process(UserProfile source, GoalExtendedStatsDto destination, ResolutionContext context)
        {
            var goals = source.Account.UserGoals;

            destination.CurrentTotal = source.ActiveGoals;
            destination.CurrentCompleted = goals.Count(g => !g.IsExpired && g.IsAchieved);
            destination.InProgress = source.ActiveGoals - destination.CurrentCompleted;
            destination.TotalCreated = source.TotalGoals;
            destination.TotalCompleted = source.CompletedGoals;
            destination.TotalExpired = source.ExpiredGoals;
        }
    }
}
