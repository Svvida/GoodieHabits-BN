using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IQuestRepository : IBaseRepository<Quest>
    {
        Task<IEnumerable<Quest>> GetActiveQuestsForDisplayAsync(int accountId, DateTime todayStart, DateTime todayEnd, SeasonEnum currentSeason, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetQuestsByTypeForDisplayAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestByIdAsync(int questId, QuestTypeEnum questType, bool asNoTracking, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestByIdForCompletionUpdateAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestByIdForUpdateAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestForDisplayAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetRepeatableQuestsAsync(bool asNoTracking, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetRepeatableQuestsForStatsProcessingAsync(DateTime utcNow, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetRepeatableQuestsForOccurrencesProcessingAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
        Task<bool> IsQuestOwnedByUserAsync(int questId, int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetQuestEligibleForGoalAsync(int accountId, DateTime now, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetQuestsEligibleForResetAsync(CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestWithAccountAsync(int questId, int accountId, CancellationToken cancellationToken = default);
    }
}
