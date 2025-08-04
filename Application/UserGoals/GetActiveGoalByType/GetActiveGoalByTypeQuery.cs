using Application.Quests.Dtos;
using Domain.Enum;
using MediatR;

namespace Application.UserGoals.GetActiveGoalByType
{
    public record GetActiveGoalByTypeQuery(int AccountId, GoalTypeEnum GoalType) : IRequest<QuestDetailsDto?>;
}
