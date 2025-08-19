namespace Application.Common.Interfaces
{
    public interface ICurrentUserQuestCommand
    {
        int AccountId { get; }
        int QuestId { get; }
    }
}
