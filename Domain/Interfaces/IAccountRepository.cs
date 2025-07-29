using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Account?> GetAccountWithProfileAsync(int accountId, CancellationToken cancellationToken = default);
        Task<bool> DoesLoginExistAsync(string login, int accountIdToExclude, CancellationToken cancellationToken = default);
        Task<bool> DoesEmailExistAsync(string email, int accountIdToExclude, CancellationToken cancellationToken = default);
        Task<Account?> GetAccountToWipeoutDataAsync(int accountId, CancellationToken cancellationToken = default);
        // In delete method, we need to manually delete Quest_QuestLabels
    }
}
