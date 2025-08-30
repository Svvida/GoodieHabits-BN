using Application.Common.Interfaces;
using Application.Quests.Dtos;

namespace Application.Quests.Queries.GetActiveQuests
{
    public record GetActiveQuestsQuery(int UserProfileId, CancellationToken CancellationToken = default) : IQuery<IEnumerable<QuestDetailsDto>>;
}
