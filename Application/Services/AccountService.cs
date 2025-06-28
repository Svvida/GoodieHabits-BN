using Application.Dtos.Accounts;
using Application.Dtos.Auth;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IPasswordHasher<Account> passwordHasher,
            ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<GetAccountDto> GetAccountWithProfileInfoAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var account = await _unitOfWork.Accounts.GetAccountWithProfileInfoAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {accountId} was not found");
            return _mapper.Map<GetAccountDto>(account);
        }

        public async Task UpdateAccountAsync(int accountId, UpdateAccountDto patchDto, CancellationToken cancellationToken = default)
        {
            var account = await _unitOfWork.Accounts.GetAccountWithProfileInfoAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {accountId} was not found");

            if (patchDto.Login is not null && !patchDto.Login.Equals(account.Login, StringComparison.OrdinalIgnoreCase))
            {
                if (await _unitOfWork.Accounts.DoesLoginExistAsync(patchDto.Login, accountId, cancellationToken).ConfigureAwait(false))
                    throw new ConflictException($"Login {patchDto.Login} is already in use");
            }

            if (patchDto.Email is not null && !patchDto.Email.Equals(account.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _unitOfWork.Accounts.DoesEmailExistAsync(patchDto.Email, accountId, cancellationToken).ConfigureAwait(false))
                    throw new ConflictException($"Email {patchDto.Email} is already in use");
            }

            if (patchDto.Nickname is not null && !patchDto.Nickname.Equals(account.Profile.Nickname, StringComparison.OrdinalIgnoreCase))
            {
                if (await _unitOfWork.UserProfiles.DoesNicknameExistAsync(patchDto.Nickname, accountId, cancellationToken).ConfigureAwait(false))
                    throw new ConflictException($"Nickname {patchDto.Nickname} is already in use");
            }

            _mapper.Map(patchDto, account);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task ChangePasswordAsync(int accountId, ChangePasswordDto resetPasswordDto, CancellationToken cancellationToken = default)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID: {accountId} not found");

            var result = _passwordHasher.VerifyHashedPassword(account, account.HashPassword, resetPasswordDto.OldPassword);
            if (result != PasswordVerificationResult.Success)
                throw new UnauthorizedException("Invalid password");

            account.HashPassword = _passwordHasher.HashPassword(account, resetPasswordDto.NewPassword);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAccountAsync(int accountId, DeleteAccountDto deleteAccountDto, CancellationToken cancellationToken = default)
        {
            var accountForAuth = await _unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID: {accountId} not found");

            var result = _passwordHasher.VerifyHashedPassword(accountForAuth, accountForAuth.HashPassword, deleteAccountDto.Password);
            if (result != PasswordVerificationResult.Success)
                throw new UnauthorizedException("Invalid password");

            // First fetch and delete labels to avoid db conflict. Needs to be done separately because doing so in one operation violates non nullability on AccountId
            var labelsToDelete = await _unitOfWork.QuestLabels.GetUserLabelsAsync(accountId, false, cancellationToken).ConfigureAwait(false);
            if (labelsToDelete.Any())
            {
                _unitOfWork.QuestLabels.DeleteRange(labelsToDelete);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            var accountToDelete = await _unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID: {accountId} not found after label deletion.");

            _unitOfWork.Accounts.Delete(accountToDelete);

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task UpdateTimeZoneIfChangedAsync(int accountId, string? timeZone, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(timeZone))
                return;

            var normalizedTimeZone = timeZone.Trim();

            if (DateTimeZoneProviders.Tzdb.GetZoneOrNull(normalizedTimeZone) is null)
            {
                _logger.LogWarning($"Invalid time zone received: {normalizedTimeZone} for user with ID: {accountId}. Not saving time zone to database.");
                return;
            }

            var account = await _unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken).ConfigureAwait(false);
            if (account is null)
                return;

            if (!string.Equals(account.TimeZone, normalizedTimeZone, StringComparison.Ordinal))
            {
                account.TimeZone = normalizedTimeZone;
                _logger.LogInformation("Updated timezone for user {UserId} to {TimeZone}.", accountId, normalizedTimeZone);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task WipeoutAccountDataAsync(PasswordConfirmationDto authRequestDto, CancellationToken cancellationToken = default)
        {
            var account = await _unitOfWork.Accounts.GetAccountToWipeoutDataAsync(authRequestDto.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {authRequestDto.AccountId} was not found");

            var result = _passwordHasher.VerifyHashedPassword(account, account.HashPassword, authRequestDto.Password);
            if (result != PasswordVerificationResult.Success)
                throw new UnauthorizedException("Invalid password");

            // Clear profile information
            account.Profile.WipeoutData();
            // Clear quests
            account.Quests.Clear();
            // Clear labels
            var labelsToDelete = account.Labels;
            _unitOfWork.QuestLabels.DeleteRange(labelsToDelete);

            var affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogDebug("Affected rows after wiping out account data: {AffectedRows}", affectedRows);
        }
    }
}
