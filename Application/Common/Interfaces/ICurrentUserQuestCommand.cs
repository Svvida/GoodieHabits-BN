namespace Application.Common.Interfaces
{
    public interface ICurrentUserQuestCommand
    {
        int UserProfileId { get; }
        int QuestId { get; }
    }
}
