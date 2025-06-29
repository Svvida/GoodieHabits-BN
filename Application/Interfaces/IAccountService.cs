using Application.Dtos.Accounts;
using Application.Dtos.Auth;

namespace Application.Interfaces
{
    public interface IAccountService
    {
        Task<GetAccountDto> GetAccountWithProfileInfoAsync(int accountId, CancellationToken cancellationToken = default);
        Task UpdateAccountAsync(int accountId, UpdateAccountDto patchDto, CancellationToken cancellationToken = default);
        Task ChangePasswordAsync(int accountId, ChangePasswordDto resetPasswordDto, CancellationToken cancellationToken = default);
        Task DeleteAccountAsync(int accountId, PasswordConfirmationDto passwordConfirmationDto, CancellationToken cancellationToken = default);
        Task UpdateTimeZoneIfChangedAsync(int accountId, string? timeZone, CancellationToken cancellationToken = default);
        Task WipeoutAccountDataAsync(PasswordConfirmationDto authRequestDto, CancellationToken cancellationToken = default);
    }
}
