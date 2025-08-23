using Domain.Interfaces;
using MediatR;

namespace Application.Accounts.Commands.VerifyPasswordResetCode
{
    public class VerifyPasswordResetCodeCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<VerifyPasswordResetCodeCommand, bool>
    {
        public async Task<bool> Handle(VerifyPasswordResetCodeCommand request, CancellationToken cancellationToken)
        {
            var user = await unitOfWork.Accounts.GetByEmailAsync(request.Email, cancellationToken).ConfigureAwait(false);

            if (user is null || user.ResetPasswordCode is null || user.ResetPasswordCodeExpiresAt < DateTime.UtcNow)
                return false;

            return user.ResetPasswordCode == request.ResetCode;
        }
    }
}
