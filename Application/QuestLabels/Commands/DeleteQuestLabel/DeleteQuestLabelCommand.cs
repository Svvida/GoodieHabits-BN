using Application.Common.Interfaces;

namespace Application.QuestLabels.Commands.DeleteQuestLabel
{
    public record DeleteQuestLabelCommand(int Id, int UserProfileId) : ICommand, ICurrentUserQuestLabelCommand;
}
