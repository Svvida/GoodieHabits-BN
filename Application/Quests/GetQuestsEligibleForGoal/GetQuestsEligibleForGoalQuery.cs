using Application.Quests.Dtos;
using MediatR;

namespace Application.Quests.GetQuestsEligibleForGoal
{
    public record GetQuestsEligibleForGoalQuery(int AccountId, CancellationToken CancellationToken) : IRequest<IEnumerable<QuestDetailsDto>>;
}
