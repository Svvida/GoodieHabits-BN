using Application.Dtos.Quests;
using MediatR;

namespace Application.Quests.Queries
{
    public record GetActiveQuestsQuery(int AccountId, CancellationToken CancellationToken = default) : IRequest<IEnumerable<BaseGetQuestDto>>;
}
