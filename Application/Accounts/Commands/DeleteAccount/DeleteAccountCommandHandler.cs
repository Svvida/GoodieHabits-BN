using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Accounts.Commands.DeleteAccount
{
    public class DeleteAccountCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher<Account> passwordHasher) : IRequestHandler<DeleteAccountCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var account = await unitOfWork.Accounts.GetByIdAsync(request.AccountId, cancellationToken).ConfigureAwait(false);

            if (account is null || passwordHasher.VerifyHashedPassword(account, account.HashPassword, request.Password) == PasswordVerificationResult.Failed)
                throw new UnauthorizedException("Invalid credentials provided.");

            // First fetch and delete labels to avoid db conflict. Needs to be done separately because doing so in one operation violates non nullability on AccountId
            var labelsToDelete = await unitOfWork.QuestLabels.GetUserLabelsAsync(request.UserProfileId, false, cancellationToken).ConfigureAwait(false);
            if (labelsToDelete.Any())
            {
                unitOfWork.QuestLabels.RemoveRange(labelsToDelete);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            unitOfWork.Accounts.Remove(account);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
