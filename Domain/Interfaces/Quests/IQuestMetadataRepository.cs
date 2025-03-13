using Domain.Enum;
using Domain.Models;

namespace Domain.Interfaces.Quests
{
    public interface IQuestMetadataRepository
    {
        Task<IEnumerable<QuestMetadata>> GetTodaysQuestsAsync(int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<QuestMetadata>> GetUserSubtypeQuestsAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default);
    }
}
