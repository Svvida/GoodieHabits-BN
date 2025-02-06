using Domain.Models;

namespace Domain.Interfaces
{
    public interface IQuestMetadataRepository
    {
        Task<IEnumerable<QuestMetadata>> GetAllQuestsAsync(CancellationToken cancellationToken = default);
    }
}
