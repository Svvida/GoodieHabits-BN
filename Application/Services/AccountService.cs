﻿using Application.Dtos.Accounts;
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

        public async Task<GetAccountDto> GetAccountByIdAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken, a => a.Labels).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {accountId} was not found");

            return _mapper.Map<GetAccountDto>(account);
        }

        public async Task PatchAccountAsync(int accountId, PatchAccountDto patchDto, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(patchDto.Nickname))
            {
                if (await _accountRepository.GetByUsernameAsync(patchDto.Nickname!, cancellationToken) is not null)
                    throw new ConflictException($"Nickname {patchDto.Nickname} is already in use");
            }

            var account = await _accountRepository.GetByIdAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {accountId} was not found");

            _mapper.Map(patchDto, account);

            await _accountRepository.UpdateAsync(account, cancellationToken).ConfigureAwait(false);
        }
    }
}
