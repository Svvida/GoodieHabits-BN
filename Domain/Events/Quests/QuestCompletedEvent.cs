namespace Domain.Events.Quests
{
    public record QuestCompletedEvent(
        int AccountId,
        int XpAwarded,
        int GoalsCompletedCount,
        bool IsFirstTimeCompleted,
        bool ShouldAssignRewards);
}
