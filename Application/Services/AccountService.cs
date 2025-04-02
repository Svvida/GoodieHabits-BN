using Application.Dtos.Accounts;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly IUserProfileRepository _userProfileRepository;

        public AccountService(
            IAccountRepository accountRepository,
            IMapper mapper,
            IUserProfileRepository userProfileRepository)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
            _userProfileRepository = userProfileRepository;
        }

        public async Task<GetAccountDto> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken, a => a.Profile).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {accountId} was not found");

            return _mapper.Map<GetAccountDto>(account);
        }

        public async Task UpdateAccountAsync(int accountId, UpdateAccountDto patchDto, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken, a => a.Profile).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {accountId} was not found");

            if (patchDto.Login is not null && patchDto.Login != account.Login)
            {
                if (await _accountRepository.ExistsByFieldAsync(a => a.Login, patchDto.Login, cancellationToken).ConfigureAwait(false))
                    throw new ConflictException($"Login {patchDto.Login} is already in use");
            }

            if (patchDto.Email is not null && patchDto.Email != account.Email)
            {
                if (await _accountRepository.ExistsByFieldAsync(a => a.Email, patchDto.Email, cancellationToken).ConfigureAwait(false))
                    throw new ConflictException($"Email {patchDto.Email} is already in use");
            }

            if (patchDto.Nickname is not null && patchDto.Nickname != account.Profile.Nickname)
            {
                if (await _userProfileRepository.ExistsByNicknameAsync(patchDto.Nickname, cancellationToken).ConfigureAwait(false))
                    throw new ConflictException($"Nickname {patchDto.Nickname} is already in use");
            }

            _mapper.Map(patchDto, account);

            await _accountRepository.UpdateAsync(account, cancellationToken).ConfigureAwait(false);
        }
    }
}
