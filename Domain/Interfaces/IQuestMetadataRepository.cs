using Domain.Models;

namespace Domain.Interfaces
{
    public interface IQuestMetadataRepository
    {
        Task<IEnumerable<QuestMetadata>> GetTodaysQuestsAsync(int accountId, CancellationToken cancellationToken = default);
    }
}
