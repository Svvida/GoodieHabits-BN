using Domain.Enum;
using MediatR;

namespace Application.Commands
{
    public record UpdateQuestCompletionCommand(
        int QuestId,
        bool IsCompleted,
        QuestTypeEnum QuestType) : IRequest<Unit>;
}
