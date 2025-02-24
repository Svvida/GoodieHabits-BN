using Application.Dtos.Accounts;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            IAccountRepository accountRepository,
            IMapper mapper,
            IPasswordHasher<Account> passwordHasher,
            ILogger<AccountService> logger)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<IEnumerable<GetAccountDto>> GetAllAccountsAsync(CancellationToken cancellationToken = default)
        {
            var accounts = await _accountRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return _mapper.Map<IEnumerable<GetAccountDto>>(accounts);
        }

        public async Task<GetAccountDto?> GetAccountByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {id} was not found");

            return _mapper.Map<GetAccountDto>(account);
        }
    }
}
