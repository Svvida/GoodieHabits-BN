using Domain.Models;

namespace Domain.Interfaces
{
    public interface IQuestMetadataRepository
    {
        Task<IEnumerable<QuestMetadata>> GetTodaysQuestsAsync(CancellationToken cancellationToken = default);
    }
}
