using Application.Dtos.Quests;
using MediatR;

namespace Application.Quests.Queries
{
    public record GetQuestsEligibleForGoalQuery(int AccountId, CancellationToken CancellationToken) : IRequest<IEnumerable<BaseGetQuestDto>>;
}
