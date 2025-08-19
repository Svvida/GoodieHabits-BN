using Application.Common.Interfaces;
using Application.Quests.Dtos;
using Domain.Enums;

namespace Application.Quests.Queries.GetQuestsByType
{
    public record GetQuestsByTypeQuery(int AccountId, QuestTypeEnum QuestType) : IQuery<IEnumerable<QuestDetailsDto>>;
}
