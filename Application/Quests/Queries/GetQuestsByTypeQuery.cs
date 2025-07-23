using Application.Dtos.Quests;
using Domain.Enum;
using MediatR;

namespace Application.Quests.Queries
{
    public record GetQuestsByTypeQuery(int AccountId, QuestTypeEnum QuestType) : IRequest<IEnumerable<BaseGetQuestDto>>;
}
