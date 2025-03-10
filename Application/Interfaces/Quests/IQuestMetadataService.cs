namespace Application.Interfaces.Quests
{
    public interface IQuestMetadataService
    {
        Task<IEnumerable<object>> GetAllQuestsAsync(int accountId, CancellationToken cancellationToken = default);
    }
}
