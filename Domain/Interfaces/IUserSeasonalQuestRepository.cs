using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IUserSeasonalQuestRepository : IBaseRepository<UserSeasonalQuest>
    {
        Task<IEnumerable<UserSeasonalQuest>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default);
    }
}
