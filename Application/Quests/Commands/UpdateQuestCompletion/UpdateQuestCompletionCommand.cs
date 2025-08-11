using Application.Common.Interfaces;
using Domain.Enum;
using MediatR;

namespace Application.Quests.Commands.UpdateQuestCompletion
{
    public record UpdateQuestCompletionCommand(
        bool IsCompleted,
        int QuestId,
        int AccountId,
        QuestTypeEnum QuestType) : IRequest<Unit>, ICurrentUserQuestCommand;
}
