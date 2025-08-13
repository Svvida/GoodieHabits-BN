using Application.Quests.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Quests.Queries.GetQuestsByType
{
    public record GetQuestsByTypeQuery(int AccountId, QuestTypeEnum QuestType) : IRequest<IEnumerable<QuestDetailsDto>>;
}
