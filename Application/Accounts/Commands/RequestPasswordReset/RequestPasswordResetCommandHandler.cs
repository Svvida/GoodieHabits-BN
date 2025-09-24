using Application.Common.Interfaces.Email;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Accounts.Commands.RequestPasswordReset
{
    public class RequestPasswordResetCommandHandler(IUnitOfWork unitOfWork, IForgotPasswordEmailSender emailSender, IClock clock) : IRequestHandler<RequestPasswordResetCommand, Unit>
    {
        public async Task<Unit> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
        {
            // Check if the account exists in the database
            var account = await unitOfWork.Accounts.GetByEmailAsync(request.Email!, cancellationToken).ConfigureAwait(false);

            if (account is null)
                // If the account does not exist, we still return success to avoid revealing whether the email is registered
                return Unit.Value;

            account.InitializePasswordReset(clock.GetCurrentInstant().ToDateTimeUtc());

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (account.ResetPasswordCode is null)
                throw new FailedToGenerateResetPasswordCodeException(account.Id);

            await emailSender.SendForgotPasswordEmailAsync(request.Email!, account.ResetPasswordCode, cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
