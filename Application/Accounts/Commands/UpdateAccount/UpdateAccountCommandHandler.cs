using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Accounts.Commands.UpdateAccount
{
    public class UpdateAccountCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateAccountCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
        {
            var account = await unitOfWork.Accounts.GetAccountWithProfileAsync(request.AccountId, cancellationToken)
                ?? throw new NotFoundException($"Account with ID {request.AccountId} not found.");

            account.UpdateLogin(request.Login);
            account.UpdateEmail(request.Email);
            account.Profile.UpdateNickname(request.Nickname);
            account.Profile.UpdateBio(request.Bio);
            return Unit.Value;
        }
    }
}
