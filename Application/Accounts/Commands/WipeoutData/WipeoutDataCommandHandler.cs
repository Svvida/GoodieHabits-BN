using Application.Common.Interfaces;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Accounts.Commands.WipeoutData
{
    public class WipeoutDataCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher<Account> passwordHasher, IPhotoService photoService, ILogger<WipeoutDataCommandHandler> logger) : IRequestHandler<WipeoutDataCommand, Unit>
    {
        public async Task<Unit> Handle(WipeoutDataCommand command, CancellationToken cancellationToken)
        {
            var userProfile = await unitOfWork.UserProfiles.GetUserProfileToWipeoutDataAsync(command.UserProfileId, cancellationToken).ConfigureAwait(false);

            if (userProfile is null || passwordHasher.VerifyHashedPassword(userProfile.Account, userProfile.Account.HashPassword, command.Password) == PasswordVerificationResult.Failed)
                throw new UnauthorizedException("Invalid credentials provided.");

            var avatarUrlToDelete = userProfile.UploadedAvatarUrl;

            userProfile.WipeoutData();

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(avatarUrlToDelete))
            {
                try
                {
                    await photoService.DeletePhotoAsync(avatarUrlToDelete);
                }
                catch (Exception)
                {
                    logger.LogError("Failed to delete avatar photo for UserProfileId {UserProfileId} during data wipeout.", command.UserProfileId);
                }
            }

            return Unit.Value;
        }
    }
}
