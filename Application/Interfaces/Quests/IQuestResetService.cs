namespace Application.Interfaces.Quests
{
    public interface IQuestResetService
    {
        Task ResetDailyQuestsAsync(CancellationToken cancellationToken = default);
    }
}
