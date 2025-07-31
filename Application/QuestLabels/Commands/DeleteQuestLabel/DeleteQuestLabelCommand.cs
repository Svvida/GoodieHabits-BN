using MediatR;

namespace Application.QuestLabels.Commands.DeleteQuestLabel
{
    public record DeleteQuestLabelCommand(int Id) : IRequest<Unit>;
}
