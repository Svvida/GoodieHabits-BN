using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateAccountCommandHandler> logger) : IRequestHandler<UpdateAccountCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = await unitOfWork.Accounts.GetAccountWithProfileAsync(request.AccountId, cancellationToken)
                ?? throw new NotFoundException($"Account with ID {request.AccountId} not found.");

            logger.LogDebug("Update Account: {AccountId}, Request Payload: {@Request}", request.AccountId, request);

            account.UpdateLogin(request.Login);
            account.UpdateEmail(request.Email);
            account.Profile.UpdateNickname(request.Nickname);
            account.Profile.UpdateBio(request.Bio);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
