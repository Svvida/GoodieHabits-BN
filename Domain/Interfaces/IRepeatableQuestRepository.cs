using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IRepeatableQuestRepository : IBaseRepository<RepeatableQuest>
    {
        Task<IEnumerable<RepeatableQuest>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RepeatableQuest>> GetDueQuestsAsync(TimeOnly currentTime, CancellationToken cancellationToken = default);

    }
}
