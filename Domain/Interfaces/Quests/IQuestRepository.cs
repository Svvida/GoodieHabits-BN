using Domain.Enum;
using Domain.Models;

namespace Domain.Interfaces.Quests
{
    public interface IQuestRepository
    {
        Task<IEnumerable<Quest>> GetActiveQuestsAsync(int accountId, SeasonEnum currentSeason, CancellationToken cancellationToken = default);
        Task<IEnumerable<Quest>> GetQuestsByTypeAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<Quest?> GetQuestByIdAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task DeleteQuestByIdAsync(int questId, CancellationToken cancellationToken = default);
        Task AddQuestLabelsAsync(List<Quest_QuestLabel> labelsToAdd, CancellationToken cancellationToken = default);
        Task RemoveQuestLabelsAsync(List<Quest_QuestLabel> labelsToRemove, CancellationToken cancellationToken = default);
        Task<bool> IsQuestOwnedByUserAsync(int questId, int accountId, CancellationToken cancellationToken = default);
        Task AddQuestAsync(Quest quest, CancellationToken cancellationToken = default);
        Task UpdateQuestAsync(Quest quest, CancellationToken cancellationToken = default);
    }
}
