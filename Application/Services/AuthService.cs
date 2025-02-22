using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Configurations;
using Application.Dtos;
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

        public AuthService(
            IOptions<JwtSettings> jwtSettings,
            IAccountRepository accountRepository,
            IPasswordHasher<Account> passwordHasher)
        {
            _jwtSettings = jwtSettings.Value ?? throw new ArgumentException($"{nameof(jwtSettings)} is missing in configuration.", nameof(jwtSettings));
            _accountRepository = accountRepository;
            _passwordHasher = passwordHasher;
        }

        private string GenerateJwtToken(Account user)
        {
            var claims = new Claim[]
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (ClaimTypes.Name, user.Username)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            var account = await _accountRepository.GetByUsernameAsync(loginDto.Login, cancellationToken)
                ?? throw new NotFoundException($"Account with login: {loginDto.Login} not found");

            var result = _passwordHasher.VerifyHashedPassword(account, account.HashPassword, loginDto.Password);

            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedException("Invalid password");

            var token = GenerateJwtToken(account);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationMinutes)
            };
        }
    }
}
