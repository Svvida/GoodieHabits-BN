using Application.Quests.Dtos;
using MediatR;

namespace Application.Quests.GetActiveQuests
{
    public record GetActiveQuestsQuery(int AccountId, CancellationToken CancellationToken = default) : IRequest<IEnumerable<QuestDetailsDto>>;
}
