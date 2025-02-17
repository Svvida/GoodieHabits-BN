using Application.Dtos.QuestMetadata;

namespace Application.Interfaces
{
    public interface IQuestMetadataService
    {
        Task<IEnumerable<GetQuestMetadataDto>> GetAllQuestsAsync(CancellationToken cancellationToken = default);
    }
}
