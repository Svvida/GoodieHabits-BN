using Domain.Enum;
using Domain.Models;

namespace Domain.Interfaces.Quests
{
    public interface IQuestMetadataRepository
    {
        Task<IEnumerable<QuestMetadata>> GetActiveQuestsAsync(int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<QuestMetadata>> GetQuestsByTypeAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<QuestMetadata?> GetQuestByIdAsync(int questId, CancellationToken cancellationToken = default);
        Task<QuestMetadata?> GetQuestMetadataByIdAsync(int questId, CancellationToken cancellationToken = default);
        Task DeleteAsync(QuestMetadata quest, CancellationToken cancellationToken = default);
        Task AddQuestLabelsAsync(List<QuestMetadata_QuestLabel> labelsToAdd, CancellationToken cancellationToken = default);
        Task RemoveQuestLabelsAsync(List<QuestMetadata_QuestLabel> labelsToRemove, CancellationToken cancellationToken = default);
        Task<bool> IsQuestOwnedByUserAsync(int questId, int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
    }
}
