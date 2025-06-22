using Domain.Models;

namespace Application.Interfaces.Quests
{
    public interface IQuestStatisticsService
    {
        Task<int> ProcessStatisticsForQuestsAndSaveAsync(CancellationToken cancellationToken = default);
        Task ProcessStatisticsForQuestAsync(Quest quest, CancellationToken cancellationToken = default);
    }
}
