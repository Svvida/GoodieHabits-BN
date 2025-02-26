namespace Application.Interfaces.Quests
{
    public interface IQuestResetService
    {
        void ResetDailyQuestsAsync(CancellationToken cancellationToken = default);
    }
}
