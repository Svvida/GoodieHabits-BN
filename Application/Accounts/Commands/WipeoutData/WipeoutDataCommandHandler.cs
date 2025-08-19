using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Accounts.Commands.WipeoutData
{
    public class WipeoutDataCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher<Account> passwordHasher) : IRequestHandler<WipeoutDataCommand, Unit>
    {
        public async Task<Unit> Handle(WipeoutDataCommand request, CancellationToken cancellationToken)
        {
            var account = await unitOfWork.Accounts.GetAccountToWipeoutDataAsync(request.AccountId, cancellationToken).ConfigureAwait(false);

            if (account is null || passwordHasher.VerifyHashedPassword(account, account.HashPassword, request.Password) == PasswordVerificationResult.Failed)
                throw new UnauthorizedException("Invalid credentials provided.");

            // Clear profile information
            account.Profile.WipeoutData();
            // Clear quests
            account.Quests.Clear();
            // Clear labels
            var labelsToDelete = account.Labels.ToList();
            unitOfWork.QuestLabels.RemoveRange(labelsToDelete);

            return Unit.Value;
        }
    }
}
