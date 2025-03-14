using Application.Dtos.Quests;

namespace Application.Interfaces.Quests
{
    public interface IQuestService
    {
        Task<IEnumerable<BaseGetQuestDto>> GetActiveQuestsAsync(int accountId, CancellationToken cancellationToken = default);
    }
}
