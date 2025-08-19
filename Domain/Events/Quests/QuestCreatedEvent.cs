using Domain.Enums;

namespace Domain.Events.Quests
{
    public record QuestCreatedEvent(int AccountId, string Title, QuestTypeEnum QuestType);
}
