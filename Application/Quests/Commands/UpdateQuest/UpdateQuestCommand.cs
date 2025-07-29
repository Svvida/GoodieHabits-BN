using Application.Dtos.Quests;
using MediatR;

namespace Application.Quests.Commands.UpdateQuest
{
    public record UpdateQuestCommand(BaseUpdateQuestDto UpdateDto, CancellationToken CancellationToken) : IRequest<BaseGetQuestDto>;
}
