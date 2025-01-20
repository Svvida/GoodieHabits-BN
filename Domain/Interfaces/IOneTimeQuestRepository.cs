using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IOneTimeQuestRepository : IBaseRepository<OneTimeQuest>
    {
        Task<IEnumerable<OneTimeQuest>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<OneTimeQuest>> GetOverdueQuestsAsync(CancellationToken cancellationToken = default);

    }
}
