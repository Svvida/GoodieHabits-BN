using System.Security.Claims;
using System.Text.RegularExpressions;
using Application.Configurations;
using Application.Dtos.Accounts;
using Application.Dtos.Auth;
using Application.Interfaces;
using Application.UserProfiles.Nickname;
using AutoMapper;
using Domain;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Authentication;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly IMapper _mapper;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ITokenValidator _tokenValidator;
        private readonly INicknameGenerator _nicknameGeneratorService;

        public AuthService(
            IOptions<JwtSettings> jwtSettings,
            IUnitOfWork unitOfWork,
            IPasswordHasher<Account> passwordHasher,
            IMapper mapper,
            ITokenGenerator tokenGenerator,
            ITokenValidator tokenValidator,
            INicknameGenerator nicknameGeneratorService)
        {
            _jwtSettings = jwtSettings.Value ?? throw new InvalidArgumentException($"{nameof(jwtSettings)} is missing in configuration.");
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _tokenGenerator = tokenGenerator;
            _tokenValidator = tokenValidator;
            _nicknameGeneratorService = nicknameGeneratorService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            Account account;
            if (IsEmail(loginDto.Login))
            {
                account = await _unitOfWork.Accounts.GetByEmailAsync(loginDto.Login, cancellationToken)
                ?? throw new UnauthorizedException("Invalid credentials.");
            }
            else
            {
                account = await _unitOfWork.Accounts.GetByUsernameAsync(loginDto.Login, cancellationToken)
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
            if (await _unitOfWork.Accounts.GetByEmailAsync(createDto.Email, cancellationToken) != null)
                throw new ConflictException($"Account with email {createDto.Email} already exists");

            var accountEntity = _mapper.Map<Account>(createDto);

            accountEntity.UpdateHashPassword(_passwordHasher.HashPassword(accountEntity, createDto.Password));

            accountEntity.Profile.Nickname = await _nicknameGeneratorService.GenerateUniqueNicknameAsync(cancellationToken).ConfigureAwait(false);

            await _unitOfWork.Accounts.AddAsync(accountEntity, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

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

            var account = await _unitOfWork.Accounts.GetByIdAsync(int.Parse(accountId), cancellationToken)
                ?? throw new NotFoundException($"Account with ID: {accountId} not found");

            return new RefreshResponseDto
            {
                AccessToken = GenerateJwtToken(account),
                AccountId = account.Id
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
