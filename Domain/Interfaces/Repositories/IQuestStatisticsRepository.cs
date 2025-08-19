using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IQuestStatisticsRepository : IBaseRepository<QuestStatistics>
    {
        Task<QuestStatistics?> GetStatisticsForQuestAsync(int questId, bool asNoTracking, CancellationToken cancellationToken = default);
    }
}
