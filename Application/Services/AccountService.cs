using Application.Dtos.Accounts;
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
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IQuestLabelRepository _questLabelRepository;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAccountRepository accountRepository,
            IMapper mapper,
            IUserProfileRepository userProfileRepository,
            IPasswordHasher<Account> passwordHasher,
            IQuestLabelRepository questLabelRepository,
            ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
            _userProfileRepository = userProfileRepository;
            _passwordHasher = passwordHasher;
            _questLabelRepository = questLabelRepository;
            _logger = logger;
        }

        public async Task<GetAccountDto> GetAccountWithProfileInfoAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetAccountWithProfileInfoAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {accountId} was not found");
            return _mapper.Map<GetAccountDto>(account);
        }

        public async Task UpdateAccountAsync(int accountId, UpdateAccountDto patchDto, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetAccountWithProfileInfoAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {accountId} was not found");

            if (patchDto.Login is not null && !patchDto.Login.Equals(account.Login, StringComparison.OrdinalIgnoreCase))
            {
                if (await _accountRepository.DoesLoginExistAsync(patchDto.Login, accountId, cancellationToken).ConfigureAwait(false))
                    throw new ConflictException($"Login {patchDto.Login} is already in use");
            }

            if (patchDto.Email is not null && !patchDto.Email.Equals(account.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _accountRepository.DoesEmailExistAsync(patchDto.Email, accountId, cancellationToken).ConfigureAwait(false))
                    throw new ConflictException($"Email {patchDto.Email} is already in use");
            }

            if (patchDto.Nickname is not null && !patchDto.Nickname.Equals(account.Profile.Nickname, StringComparison.OrdinalIgnoreCase))
            {
                if (await _userProfileRepository.DoesNicknameExistAsync(patchDto.Nickname, cancellationToken).ConfigureAwait(false))
                    throw new ConflictException($"Nickname {patchDto.Nickname} is already in use");
            }

            _mapper.Map(patchDto, account);

            _accountRepository.Update(account);
        }

        public async Task ChangePasswordAsync(int accountId, ChangePasswordDto resetPasswordDto, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID: {accountId} not found");

            var result = _passwordHasher.VerifyHashedPassword(account, account.HashPassword, resetPasswordDto.OldPassword);
            if (result != PasswordVerificationResult.Success)
                throw new UnauthorizedException("Invalid password");

            account.HashPassword = _passwordHasher.HashPassword(account, resetPasswordDto.NewPassword);

            _accountRepository.Update(account);
        }

        public async Task DeleteAccountAsync(int accountId, DeleteAccountDto deleteAccountDto, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID: {accountId} not found");

            var result = _passwordHasher.VerifyHashedPassword(account, account.HashPassword, deleteAccountDto.Password);
            if (result != PasswordVerificationResult.Success)
                throw new UnauthorizedException("Invalid password");

            await _questLabelRepository.DeleteQuestLabelsByAccountIdAsync(accountId, cancellationToken).ConfigureAwait(false);
            _accountRepository.Delete(account);
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

            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken).ConfigureAwait(false);
            if (account is null)
                return;

            if (!string.Equals(account.TimeZone, normalizedTimeZone, StringComparison.Ordinal))
            {
                account.TimeZone = normalizedTimeZone;
                _accountRepository.Update(account);
                _logger.LogInformation("Updated timezone for user {UserId} to {TimeZone}.", accountId, normalizedTimeZone);
            }
        }
    }
}
