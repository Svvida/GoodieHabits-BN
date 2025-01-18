using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IRecurringQuestRepository : IBaseRepository<RecurringQuest>
    {
        Task<IEnumerable<RecurringQuest>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RecurringQuest>> GetDueQuestsAsync(TimeOnly currentTime, CancellationToken cancellationToken = default);
        Task<IEnumerable<RecurringQuest>> GetImportantQuestsAsync(CancellationToken cancellationToken = default); 

    }
}
