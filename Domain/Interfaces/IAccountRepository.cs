using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IAccountRepository : IBaseRepository<Account>
    {
        Task<Account?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
