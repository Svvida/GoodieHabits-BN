using Application.Dtos.Accounts;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<GetAccountDto> GetAccountByIdAsync(int id, CancellationToken cancellationToken = default);
        Task UpdateAccountAsync(int accountId, UpdateAccountDto patchDto, CancellationToken cancellationToken = default);
    }
}
