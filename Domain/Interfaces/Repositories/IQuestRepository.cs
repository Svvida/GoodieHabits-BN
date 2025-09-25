using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IQuestRepository : IBaseRepository<Quest>
    {
        Task<IEnumerable<Quest>> GetActiveQuestsForDisplayAsync(int userProfileId, DateTime todayStart, DateTime todayEnd, SeasonEnum currentSeason, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetQuestsByTypeForDisplayAsync(int userProfileId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestByIdAsync(int questId, int userProfileId, QuestTypeEnum questType, bool asNoTracking, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestByIdForCompletionUpdateAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestByIdForUpdateAsync(int questId, int userProfileId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetRepeatableQuestsForStatsProcessingAsync(DateTime utcNow, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetRepeatableQuestsForOccurrencesProcessingAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
        Task<bool> IsQuestOwnedByUserAsync(int questId, int userProfileId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetQuestEligibleForGoalAsync(int userProfileId, DateTime now, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestWithUserProfileAsync(int questId, int userProfileId, CancellationToken cancellationToken = default);
        Task<Quest?> GetUserQuestByIdAsync(int questId, int userProfileId, CancellationToken cancellationToken = default);
    }
}
