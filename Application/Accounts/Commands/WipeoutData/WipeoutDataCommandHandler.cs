using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Accounts.Commands.WipeoutData
{
    public class WipeoutDataCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher<Account> passwordHasher) : IRequestHandler<WipeoutDataCommand, Unit>
    {
        public async Task<Unit> Handle(WipeoutDataCommand command, CancellationToken cancellationToken)
        {
            var userProfile = await unitOfWork.UserProfiles.GetUserProfileToWipeoutDataAsync(command.UserProfileId, cancellationToken).ConfigureAwait(false);

            if (userProfile is null || passwordHasher.VerifyHashedPassword(userProfile.Account, userProfile.Account.HashPassword, command.Password) == PasswordVerificationResult.Failed)
                throw new UnauthorizedException("Invalid credentials provided.");

            // Clear profile information
            userProfile.WipeoutData();
            // Clear quests
            userProfile.Quests.Clear();
            // Clear labels
            var labelsToDelete = userProfile.Labels.ToList();
            unitOfWork.QuestLabels.RemoveRange(labelsToDelete);

            return Unit.Value;
        }
    }
}
