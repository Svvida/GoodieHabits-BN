using Application.Dtos.QuestMetadata;

namespace Application.Interfaces
{
    public interface IQuestMetadataService
    {
        Task<IEnumerable<QuestMetadataDto>> GetAllQuestsAsync(CancellationToken cancellationToken = default);
    }
}
