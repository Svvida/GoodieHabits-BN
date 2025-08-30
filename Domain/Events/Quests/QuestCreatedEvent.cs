using Domain.Enums;

namespace Domain.Events.Quests
{
    public record QuestCreatedEvent(int UserProfileId, string Title, QuestTypeEnum QuestType);
}
