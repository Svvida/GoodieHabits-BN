namespace Domain.Events
{
    public record QuestDeletedEvent(
        int QuestId,
        int AccountId,
        bool IsQuestCompleted,
        bool IsQuestEverCompleted);
}
