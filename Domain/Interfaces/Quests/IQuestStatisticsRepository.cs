using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Quests
{
    public interface IQuestStatisticsRepository : IBaseRepository<QuestStatistics>
    {
        Task<QuestStatistics?> GetStatisticsForQuestAsync(int questId, bool asNoTracking, CancellationToken cancellationToken = default);
    }
}
