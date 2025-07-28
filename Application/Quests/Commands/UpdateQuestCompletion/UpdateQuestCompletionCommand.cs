using Domain.Enum;
using MediatR;

namespace Application.Quests.Commands.UpdateQuestCompletion
{
    public record UpdateQuestCompletionCommand(
        int QuestId,
        bool IsCompleted,
        QuestTypeEnum QuestType) : IRequest<Unit>;
}
