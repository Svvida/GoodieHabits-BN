using Application.UserProfiles.Nickname;
using Domain.Interfaces;
using Domain.Interfaces.Authentication;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Auth.Register
{
    public class RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher<Account> passwordHasher,
        ITokenGenerator tokenGenerator,
        INicknameGenerator nicknameGenerator) : IRequestHandler<RegisterCommand, RegisterResponse>
    {
        public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var hashedPassword = passwordHasher.HashPassword(null!, request.Password);

            var account = Account.Create(hashPassword: hashedPassword, email: request.Email, timeZone: request.TimeZoneId ?? "Etc/UTC");

            account.Profile.UpdateNickname(await nicknameGenerator.GenerateUniqueNicknameAsync(cancellationToken).ConfigureAwait(false));

            await unitOfWork.Accounts.AddAsync(account, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return new RegisterResponse(
                AccessToken: tokenGenerator.GenerateAccessToken(account),
                RefreshToken: tokenGenerator.GenerateRefreshToken(account));
        }
    }
}
