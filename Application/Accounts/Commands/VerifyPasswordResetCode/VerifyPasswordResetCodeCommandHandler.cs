using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Accounts.Commands.VerifyPasswordResetCode
{
    public class VerifyPasswordResetCodeCommandHandler(IUnitOfWork unitOfWork, IClock clock) : IRequestHandler<VerifyPasswordResetCodeCommand, bool>
    {
        public async Task<bool> Handle(VerifyPasswordResetCodeCommand request, CancellationToken cancellationToken)
        {
            var user = await unitOfWork.Accounts.GetByEmailAsync(request.Email, cancellationToken).ConfigureAwait(false);

            var utcNow = clock.GetCurrentInstant().ToDateTimeUtc();

            if (user is null || user.ResetPasswordCode is null || user.ResetPasswordCodeExpiresAt < utcNow)
                return false;

            return user.ResetPasswordCode == request.ResetCode;
        }
    }
}
