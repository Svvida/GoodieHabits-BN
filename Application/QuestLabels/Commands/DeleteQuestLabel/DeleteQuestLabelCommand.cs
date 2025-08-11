using Application.Common.Interfaces;
using MediatR;

namespace Application.QuestLabels.Commands.DeleteQuestLabel
{
    public record DeleteQuestLabelCommand(int Id, int AccountId) : IRequest<Unit>, ICurrentUserQuestLabelCommand;
}
