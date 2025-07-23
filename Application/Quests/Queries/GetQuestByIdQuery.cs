using Application.Dtos.Quests;
using Domain.Enum;
using MediatR;

namespace Application.Quests.Queries
{
    public record GetQuestByIdQuery(int QuestId, QuestTypeEnum QuestType) : IRequest<BaseGetQuestDto?>;
}
