using MediatR;

namespace Application.Commands
{
    public record DeleteQuestCommand(int QuestId) : IRequest<Unit>;
}
