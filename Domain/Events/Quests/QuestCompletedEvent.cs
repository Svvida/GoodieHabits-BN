using Domain.Enums;

namespace Domain.Events.Quests
{
    public record QuestCompletedEvent(
        int UserProfileId,
        int XpAwarded,
        bool IsGoalCompleted,
        bool IsFirstTimeCompleted,
        bool ShouldAssignRewards,
        QuestTypeEnum QuestType);
}
