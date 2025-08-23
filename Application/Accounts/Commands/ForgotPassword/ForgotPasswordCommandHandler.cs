using Application.Common.Interfaces.Email;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Accounts.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IForgotPasswordEmailSender emailSender, IClock clock) : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // Check if the user exists in the database
            var user = await unitOfWork.Accounts.GetByEmailAsync(request.Email, cancellationToken).ConfigureAwait(false);

            if (user is null)
            {
                // If the user does not exist, we still return success to avoid revealing whether the email is registered
                return Unit.Value;
            }

            user.InitializePasswordReset(clock.GetCurrentInstant().ToDateTimeUtc());

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            if (user.ResetPasswordCode is null)
                throw new FailedToGenerateResetPasswordCodeException(user.Id);

            await emailSender.SendForgotPasswordEmailAsync(request.Email, user.ResetPasswordCode.Value, cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
