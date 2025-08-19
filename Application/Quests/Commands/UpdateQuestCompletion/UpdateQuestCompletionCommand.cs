using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.Quests.Commands.UpdateQuestCompletion
{
    public record UpdateQuestCompletionCommand(
        bool IsCompleted,
        int QuestId,
        int AccountId,
        QuestTypeEnum QuestType) : ICommand<Unit>, ICurrentUserQuestCommand;
}
