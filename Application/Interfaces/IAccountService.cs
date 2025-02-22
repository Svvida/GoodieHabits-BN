using Application.Dtos.Accounts;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<IEnumerable<GetAccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken = default);
        Task<GetAccountDto?> GetAccountByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<int> CreateAccountAsync(CreateAccountDto account, CancellationToken cancellationToken = default);
    }
}
