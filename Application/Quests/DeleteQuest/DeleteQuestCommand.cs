using Application.Common.Interfaces;
using MediatR;

namespace Application.Quests.DeleteQuest
{
    public record DeleteQuestCommand(int QuestId, int AccountId) : IRequest<Unit>, ICurrentUserQuestCommand;
}
