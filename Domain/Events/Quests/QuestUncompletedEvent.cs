using Domain.Enums;

namespace Domain.Events.Quests
{
    public record QuestUncompletedEvent(
        int AccountId,
        QuestTypeEnum QuestType);
}
