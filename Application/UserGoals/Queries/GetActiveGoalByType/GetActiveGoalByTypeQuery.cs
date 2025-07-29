using Application.Dtos.Quests;
using Domain.Enum;
using MediatR;

namespace Application.UserGoals.Queries.GetActiveGoalByType
{
    public record GetActiveGoalByTypeQuery(int AccountId, GoalTypeEnum GoalType) : IRequest<BaseGetQuestDto?>;
}
