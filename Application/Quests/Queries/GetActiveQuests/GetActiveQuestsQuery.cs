using Application.Common.Interfaces;
using Application.Quests.Dtos;

namespace Application.Quests.Queries.GetActiveQuests
{
    public record GetActiveQuestsQuery(int UserProfileId) : IQuery<IEnumerable<QuestDetailsDto>>;
}
