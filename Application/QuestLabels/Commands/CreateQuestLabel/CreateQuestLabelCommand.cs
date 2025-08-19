using Application.Common.Interfaces;

namespace Application.QuestLabels.Commands.CreateQuestLabel
{
    public record CreateQuestLabelCommand(string Value, string BackgroundColor, int AccountId) : ICommand<CreateQuestLabelResponse>;
}
