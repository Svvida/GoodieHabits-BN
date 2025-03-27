using Application.Dtos.Accounts;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<GetAccountDto> GetAccountByIdAsync(int id, CancellationToken cancellationToken = default);
        Task PatchAccountAsync(int accountId, PatchAccountDto patchDto, CancellationToken cancellationToken = default);
    }
}
