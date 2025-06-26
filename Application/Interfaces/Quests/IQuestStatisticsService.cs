using Domain.Models;

namespace Application.Interfaces.Quests
{
    public interface IQuestStatisticsService
    {
        Task ProcessStatisticsForQuestAndSaveAsync(int questId, CancellationToken cancellationToken = default);
        Task<int> ProcessStatisticsForQuestsAndSaveAsync(CancellationToken cancellationToken = default);
        Task ProcessStatisticsForQuestAsync(Quest quest, CancellationToken cancellationToken = default);
    }
}
