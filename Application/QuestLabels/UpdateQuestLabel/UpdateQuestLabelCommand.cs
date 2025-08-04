using Application.Common.Interfaces;
using MediatR;

namespace Application.QuestLabels.UpdateQuestLabel
{
    public record UpdateQuestLabelCommand(string Value, string BackgroundColor, int LabelId, int AccountId) : IRequest<UpdateQuestLabelResponse>, ICurrentUserQuestLabelCommand;
}
