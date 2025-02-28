using Application.Dtos.Quests.QuestMetadata;

namespace Application.Interfaces.Quests
{
    public interface IQuestMetadataService
    {
        Task<IEnumerable<GetQuestMetadataDto>> GetAllQuestsAsync(int accountId, CancellationToken cancellationToken = default);
    }
}
