using Application.Common.Interfaces;

namespace Application.QuestLabels.Commands.DeleteQuestLabel
{
    public record DeleteQuestLabelCommand(int Id, int AccountId) : ICommand, ICurrentUserQuestLabelCommand;
}
