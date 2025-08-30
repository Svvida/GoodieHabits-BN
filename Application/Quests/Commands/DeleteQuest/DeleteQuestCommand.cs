using Application.Common.Interfaces;

namespace Application.Quests.Commands.DeleteQuest
{
    public record DeleteQuestCommand(int QuestId, int UserProfileId) : ICommand, ICurrentUserQuestCommand;
}
