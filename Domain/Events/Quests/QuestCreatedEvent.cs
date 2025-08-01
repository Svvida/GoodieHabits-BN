using Domain.Enum;

namespace Domain.Events.Quests
{
    public record QuestCreatedEvent(int AccountId, string Title, QuestTypeEnum QuestType);
}
