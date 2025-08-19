using Application.Common.Interfaces;
using Application.Quests.Dtos;
using Domain.Enums;

namespace Application.UserGoals.Queries.GetActiveGoalByType
{
    public record GetActiveGoalByTypeQuery(int AccountId, GoalTypeEnum GoalType) : IQuery<QuestDetailsDto?>;
}
