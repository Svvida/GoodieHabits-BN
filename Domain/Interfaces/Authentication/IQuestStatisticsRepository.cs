using Domain.Models;

namespace Domain.Interfaces.Authentication
{
    public interface IQuestStatisticsRepository
    {
        Task UpdateQuestStatisticsAsync(QuestStatistics stats, CancellationToken cancellationToken = default);
    }
}
