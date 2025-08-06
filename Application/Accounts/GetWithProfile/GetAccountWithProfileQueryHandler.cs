using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Accounts.GetWithProfile
{
    public class GetAccountWithProfileQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetAccountWithProfileQuery, GetAccountWithProfileResponse>
    {
        public async Task<GetAccountWithProfileResponse> Handle(GetAccountWithProfileQuery request, CancellationToken cancellationToken)
        {
            var accountWithProfile = await unitOfWork.Accounts.GetAccountWithProfileAsync(request.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {request.AccountId} was not found.");
            return mapper.Map<GetAccountWithProfileResponse>(accountWithProfile);
        }
    }
}
