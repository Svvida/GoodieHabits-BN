using Domain.Enum;
using Domain.Models;

namespace Domain.Interfaces.Quests
{
    public interface IQuestMetadataRepository
    {
        Task<IEnumerable<QuestMetadata>> GetActiveQuestsAsync(int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<QuestMetadata>> GetSubtypeQuestsAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
        Task<QuestMetadata?> GetQuestByIdAsync(int questId, CancellationToken cancellationToken = default);
    }
}
