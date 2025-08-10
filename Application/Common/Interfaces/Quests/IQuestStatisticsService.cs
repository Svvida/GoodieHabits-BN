namespace Application.Common.Interfaces.Quests
{
    public interface IQuestStatisticsService
    {
        Task ProcessStatisticsForQuestAndSaveAsync(int questId, CancellationToken cancellationToken = default);
        Task<int> ProcessStatisticsForQuestsAndSaveAsync(CancellationToken cancellationToken = default);
    }
}
