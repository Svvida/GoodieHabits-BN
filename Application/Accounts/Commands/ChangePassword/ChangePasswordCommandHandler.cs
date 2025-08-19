using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Accounts.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher<Account> passwordHasher) : IRequestHandler<ChangePasswordCommand, Unit>
    {
        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var account = await unitOfWork.Accounts.GetByIdAsync(request.AccountId, cancellationToken).ConfigureAwait(false);

            if (account is null || passwordHasher.VerifyHashedPassword(account, account.HashPassword, request.OldPassword) == PasswordVerificationResult.Failed)
                throw new UnauthorizedException("Invalid credentials provided.");

            account.UpdateHashPassword(passwordHasher.HashPassword(account, request.NewPassword));

            return Unit.Value;
        }
    }
}
