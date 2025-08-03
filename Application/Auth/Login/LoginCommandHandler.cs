using Application.Common;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Authentication;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Auth.Login
{
    public class LoginCommandHandler(IUnitOfWork unitOfWork, PasswordHasher<Account> passwordHasher, ITokenGenerator tokenGenerator) : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            Account? account;
            if (Checkers.IsEmail(request.Login))
            {
                account = await unitOfWork.Accounts.GetByEmailAsync(request.Login, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                account = await unitOfWork.Accounts.GetByUsernameAsync(request.Login, cancellationToken).ConfigureAwait(false);
            }

            if (account is null || passwordHasher.VerifyHashedPassword(account, account.HashPassword, request.Password) == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedException("Invalid credentials provided.");
            }

            return new LoginResponseDto
            {
                AccessToken = tokenGenerator.GenerateAccessToken(account),
                RefreshToken = tokenGenerator.GenerateRefreshToken(account),
            };
        }
    }
}
