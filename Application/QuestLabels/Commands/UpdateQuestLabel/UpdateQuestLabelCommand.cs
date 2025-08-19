using Application.Common.Interfaces;

namespace Application.QuestLabels.Commands.UpdateQuestLabel
{
    public record UpdateQuestLabelCommand(string Value, string BackgroundColor, int LabelId, int AccountId) : ICommand<UpdateQuestLabelResponse>, ICurrentUserQuestLabelCommand;
}
