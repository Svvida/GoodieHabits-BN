using Domain.Enum;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Quests
{
    public interface IQuestRepository : IBaseRepository<Quest>
    {
        Task<IEnumerable<Quest>> GetActiveQuestsForDisplayAsync(int accountId, DateTime todayStart, DateTime todayEnd, SeasonEnum currentSeason, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetQuestsByTypeForDisplayAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestByIdAsync(int questId, QuestTypeEnum questType, bool asNoTracking, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestForDisplayAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetRepeatableQuestsAsync(bool asNoTracking, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetRepeatableQuestsForStatsProcessingAsync(CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestForStatsProcessingAsync(int questId, CancellationToken cancellationToken = default);
        Task<bool> IsQuestOwnedByUserAsync(int questId, int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetQuestEligibleForGoalAsync(int accountId, DateTime now, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetQuestsEligibleForResetAsync(CancellationToken cancellationToken = default);
    }
}
