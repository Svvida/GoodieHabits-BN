using Application.Common.Interfaces;
using MediatR;

namespace Application.Quests.Commands.DeleteQuest
{
    public record DeleteQuestCommand(int QuestId, int AccountId) : IRequest<Unit>, ICurrentUserQuestCommand;
}
