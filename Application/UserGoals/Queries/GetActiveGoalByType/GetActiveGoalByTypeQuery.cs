using Application.Quests.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.UserGoals.Queries.GetActiveGoalByType
{
    public record GetActiveGoalByTypeQuery(int AccountId, GoalTypeEnum GoalType) : IRequest<QuestDetailsDto?>;
}
