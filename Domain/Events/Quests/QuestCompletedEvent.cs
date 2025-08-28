using Domain.Enums;

namespace Domain.Events.Quests
{
    public record QuestCompletedEvent(
        int AccountId,
        int XpAwarded,
        bool IsGoalCompleted,
        bool IsFirstTimeCompleted,
        bool ShouldAssignRewards,
        QuestTypeEnum QuestType);
}
