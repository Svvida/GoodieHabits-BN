using Application.Dtos.Quests;
using MediatR;

namespace Application.Quests.Commands.CreateQuest
{
    public record CreateQuestCommand(BaseCreateQuestDto CreateDto, CancellationToken CancellationToken) : IRequest<BaseGetQuestDto>;
}
