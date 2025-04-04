using System.Security.Claims;
using System.Text.RegularExpressions;
using Application.Configurations;
using Application.Dtos.Accounts;
using Application.Dtos.Auth;
using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Authentication;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ITokenValidator _tokenValidator;

        public AuthService(
            IOptions<JwtSettings> jwtSettings,
            IAccountRepository accountRepository,
            IPasswordHasher<Account> passwordHasher,
            IMapper mapper,
            ILogger<AuthService> logger,
            ITokenGenerator tokenGenerator,
            ITokenValidator tokenValidator)
        {
            _jwtSettings = jwtSettings.Value ?? throw new InvalidArgumentException($"{nameof(jwtSettings)} is missing in configuration.");
            _accountRepository = accountRepository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _logger = logger;
            _tokenGenerator = tokenGenerator;
            _tokenValidator = tokenValidator;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            Account account;
            if (IsEmail(loginDto.Login))
            {
                account = await _accountRepository.GetByEmailAsync(loginDto.Login, cancellationToken)
                ?? throw new UnauthorizedException("Invalid credentials.");
            }
            else
            {
                account = await _accountRepository.GetByUsernameAsync(loginDto.Login, cancellationToken)
                ?? throw new UnauthorizedException("Invalid credentials.");
            }

            var result = _passwordHasher.VerifyHashedPassword(account, account.HashPassword, loginDto.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedException("Invalid credentials.");

            var response = new AuthResponseDto
            {
                AccessToken = GenerateJwtToken(account),
                RefreshToken = GenerateRefreshToken(account)
            };
            return response;
        }

        public async Task<AuthResponseDto> RegisterAsync(CreateAccountDto createDto, CancellationToken cancellationToken = default)
        {
            if (await _accountRepository.GetByEmailAsync(createDto.Email, cancellationToken) != null)
                throw new ConflictException($"Account with email {createDto.Email} already exists");

            var accountEntity = _mapper.Map<Account>(createDto);

            accountEntity.HashPassword = _passwordHasher.HashPassword(accountEntity, createDto.Password);

            await _accountRepository.AddAsync(accountEntity, cancellationToken).ConfigureAwait(false);

            return new AuthResponseDto
            {
                AccessToken = GenerateJwtToken(accountEntity),
                RefreshToken = GenerateRefreshToken(accountEntity)
            };
        }

        public async Task<RefreshResponseDto> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            ClaimsPrincipal principal = _tokenValidator.ValidateRefreshToken(refreshToken, _jwtSettings.Issuer, _jwtSettings.RefreshToken.Key);

            var accountId = principal.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountId))
                throw new UnauthorizedException("Invalid refresh token");

            var account = await _accountRepository.GetByIdAsync(int.Parse(accountId), cancellationToken)
                ?? throw new NotFoundException($"Account with ID: {accountId} not found");

            return new RefreshResponseDto
            {
                AccessToken = GenerateJwtToken(account)
            };
        }

        private string GenerateJwtToken(Account account)
        {
            return _tokenGenerator.GenerateJwtToken(
                account,
                _jwtSettings.Issuer,
                _jwtSettings.Audience,
                _jwtSettings.AccessToken.Key,
                _jwtSettings.AccessToken.ExpirationMinutes
                );
        }

        private string GenerateRefreshToken(Account account)
        {
            return _tokenGenerator.GenerateRefreshToken(
                account,
                _jwtSettings.Issuer,
                _jwtSettings.RefreshToken.Key,
                _jwtSettings.RefreshToken.ExpirationDays
                );
        }
        private static bool IsEmail(string login) =>
            Regex.IsMatch(login, @"^(?!.*\.\.)[a-zA-Z0-9][a-zA-Z0-9._%+-]*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
    }
}
