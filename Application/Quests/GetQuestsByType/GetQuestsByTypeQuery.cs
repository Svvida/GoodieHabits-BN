using Application.Quests.Dtos;
using Domain.Enum;
using MediatR;

namespace Application.Quests.GetQuestsByType
{
    public record GetQuestsByTypeQuery(int AccountId, QuestTypeEnum QuestType) : IRequest<IEnumerable<QuestDetailsDto>>;
}
