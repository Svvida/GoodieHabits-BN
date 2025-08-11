using Application.Common.Interfaces;
using MediatR;

namespace Application.QuestLabels.Commands.UpdateQuestLabel
{
    public record UpdateQuestLabelCommand(string Value, string BackgroundColor, int LabelId, int AccountId) : IRequest<UpdateQuestLabelResponse>, ICurrentUserQuestLabelCommand;
}
