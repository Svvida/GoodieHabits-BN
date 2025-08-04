using MediatR;

namespace Application.Quests.DeleteQuest
{
    public record DeleteQuestCommand(int QuestId) : IRequest<Unit>;
}
