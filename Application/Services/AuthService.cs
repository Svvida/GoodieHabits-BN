using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Application.Configurations;
using Application.Dtos.Accounts;
using Application.Dtos.Auth;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IMapper _mapper;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IOptions<JwtSettings> jwtSettings,
            IAccountRepository accountRepository,
            IPasswordHasher<Account> passwordHasher,
            IMapper mapper,
            JwtSecurityTokenHandler jwtSecurityTokenHandler,
            ILogger<AuthService> logger)
        {
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentException($"{nameof(jwtSettings)} is missing in configuration.", nameof(jwtSettings));
            _accountRepository = accountRepository;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
            _logger = logger;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            Account account;
            if (IsEmail(loginDto.Login))
            {
                account = await _accountRepository.GetByEmailAsync(loginDto.Login, cancellationToken)
                ?? throw new NotFoundException($"Account with email: {loginDto.Login} not found");
            }
            else
            {
                account = await _accountRepository.GetByUsernameAsync(loginDto.Login, cancellationToken)
                ?? throw new NotFoundException($"Account with login: {loginDto.Login} not found");
            }

            var result = _passwordHasher.VerifyHashedPassword(account, account.HashPassword, loginDto.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedException("Invalid password");

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
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new InvalidArgumentException("Refresh token is required.");

            if (!_jwtSecurityTokenHandler.CanReadToken(refreshToken))
                throw new InvalidArgumentException("Invalid refresh token format.");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidIssuer = _jwtSettings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.RefreshToken.Key)),
            };

            SecurityToken validatedToken;
            ClaimsPrincipal? principal;
            try
            {
                principal = _jwtSecurityTokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out validatedToken);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError("Invalid token payload: {ex}", ex);
                throw new InvalidArgumentException("Invalid refresh token.");
            }

            var accountId = principal.FindFirst(JwtClaimTypes.AccountId)?.Value;
            if (string.IsNullOrWhiteSpace(accountId))
                throw new UnauthorizedException("Invalid refresh token: missing account identifier.");

            var account = await _accountRepository.GetByIdAsync(int.Parse(accountId), cancellationToken)
                ?? throw new NotFoundException($"Account with id: {accountId} not found");

            return new RefreshResponseDto
            {
                AccessToken = GenerateJwtToken(account)
            };
        }

        private string GenerateJwtToken(Account account)
        {
            var claims = new Claim[]
            {
                new (JwtClaimTypes.AccountId, account.Id.ToString()),
                new (JwtClaimTypes.Email, account.Email)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.AccessToken.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessToken.ExpirationMinutes),
                signingCredentials: credentials
                );
            return _jwtSecurityTokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken(Account account)
        {
            var claims = new Claim[]
            {
                new (JwtClaimTypes.AccountId, account.Id.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.RefreshToken.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                issuer: _jwtSettings.Issuer,
                expires: DateTime.UtcNow.AddDays(_jwtSettings.RefreshToken.ExpirationDays),
                signingCredentials: credentials
                );

            return _jwtSecurityTokenHandler.WriteToken(token);
        }

        private static bool IsEmail(string login) =>
            Regex.IsMatch(login, @"^(?!.*\.\.)[a-zA-Z0-9][a-zA-Z0-9._%+-]*@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
    }

    public static class JwtClaimTypes
    {
        public const string AccountId = "accountId";
        public const string Email = "email";
    }
}
