using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Application.Accounts.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher<Account> passwordHasher, ILogger<ResetPasswordCommandHandler> logger) : IRequestHandler<ResetPasswordCommand, Unit>
    {
        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var account = await unitOfWork.Accounts.GetByEmailAsync(request.Email, cancellationToken).ConfigureAwait(false);

            if (account is null || account.ResetPasswordCode != request.ResetCode || account.ResetPasswordCodeExpiresAt < DateTime.UtcNow)
            {
                logger.LogWarning("Failed password reset attempt for email: {Email}. Provided code: {ResetCode}.", request.Email, request.ResetCode);
                throw new InvalidCredentialsException("Invalid email or reset code.");
            }

            var hashedPassword = passwordHasher.HashPassword(account, request.NewPassword);
            account.ResetPassword(hashedPassword);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
