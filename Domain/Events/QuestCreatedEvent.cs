using Domain.Enum;

namespace Domain.Events
{
    public record QuestCreatedEvent(int AccountId, string Title, QuestTypeEnum QuestType);
}
