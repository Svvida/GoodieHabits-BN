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
        // Used in register validator
        Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken = default);
        Task<Account?> GetAccountToWipeoutDataAsync(int accountId, CancellationToken cancellationToken = default);
        Task<Account?> GetByLoginIdentifier(string loginIdentifier, CancellationToken cancellationToken = default);
        Task<IEnumerable<Account>> GetAccountsWithGoalsToExpireAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
        Task<IEnumerable<Account>> GetAccountsWithQuestsToResetAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
        // In delete method, we need to manually delete Quest_QuestLabels
    }
}
