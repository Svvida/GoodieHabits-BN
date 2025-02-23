using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Configurations;
using Application.Dtos.Auth;
using Application.Interfaces;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IAccountRepository _accountRepository;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();

        public AuthService(
            IOptions<JwtSettings> jwtSettings,
            IAccountRepository accountRepository,
            IPasswordHasher<Account> passwordHasher)
        {
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentException($"{nameof(jwtSettings)} is missing in configuration.", nameof(jwtSettings));
            _accountRepository = accountRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByEmailAsync(loginDto.Login, cancellationToken)
                ?? throw new NotFoundException($"Account with login: {loginDto.Login} not found");

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
            var principal = _jwtSecurityTokenHandler.ValidateToken(refreshToken, tokenValidationParameters, out validatedToken);

            var accountId = principal.FindFirst("accountId")?.Value;
            if (string.IsNullOrWhiteSpace(accountId))
                throw new UnauthorizedException("Invalid refresh token");

            var account = await _accountRepository.GetByIdAsync(int.Parse(accountId), cancellationToken)
                ?? throw new NotFoundException($"Account with id: {accountId} not found");

            return new RefreshResponseDto
            {
                AccessToken = GenerateJwtToken(account)
            };
        }

        private string GenerateJwtToken(Account user)
        {
            var claims = new Claim[]
            {
                new ("accountId", user.Id.ToString()),
                new ("email", user.Email)
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
                new ("accountId", account.Id.ToString())
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
    }
}
