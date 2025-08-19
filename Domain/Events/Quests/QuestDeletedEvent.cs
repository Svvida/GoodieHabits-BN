namespace Domain.Events.Quests
{
    public record QuestDeletedEvent(
        int QuestId,
        int AccountId,
        bool IsQuestCompleted,
        bool IsQuestEverCompleted);
}
