using Application.Dtos.Accounts;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<GetAccountDto> GetAccountByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<GetAccountDto> GetAccountWithProfileInfoAsync(int accountId, CancellationToken cancellationToken = default);
        Task UpdateAccountAsync(int accountId, UpdateAccountDto patchDto, CancellationToken cancellationToken = default);
        Task ChangePasswordAsync(int accountId, ChangePasswordDto resetPasswordDto, CancellationToken cancellationToken = default);
        Task DeleteAccountAsync(int accountId, DeleteAccountDto deleteAccountDto, CancellationToken cancellationToken = default);
        Task UpdateTimeZoneIfChangedAsync(int accountId, string? timeZone, CancellationToken cancellationToken = default);
    }
}
