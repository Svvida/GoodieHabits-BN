namespace Domain.Events.Quests
{
    public record QuestDeletedEvent(
        int QuestId,
        int UserProfileId,
        bool IsQuestCompleted,
        bool IsQuestEverCompleted);
}
