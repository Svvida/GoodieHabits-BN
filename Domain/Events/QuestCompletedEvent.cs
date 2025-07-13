namespace Domain.Events
{
    public record QuestCompletedEvent(
        int AccountId,
        int XpAwarded,
        int GoalsCompletedCount,
        bool IsFirstTimeCompleted,
        bool WasCompletedToday);
}
