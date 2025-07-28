using MediatR;

namespace Application.Quests.Commands.DeleteQuest
{
    public record DeleteQuestCommand(int QuestId) : IRequest<Unit>;
}
