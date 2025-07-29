using Application.Dtos.Accounts;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Accounts.Queries.GetWithProfile
{
    public class GetAccountWithProfileQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetAccountWithProfileQuery, GetAccountWithProfileDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        public async Task<GetAccountWithProfileDto> Handle(GetAccountWithProfileQuery request, CancellationToken cancellationToken)
        {
            var accountWithProfile = await _unitOfWork.Accounts.GetAccountWithProfileAsync(request.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {request.AccountId} was not found.");
            return _mapper.Map<GetAccountWithProfileDto>(accountWithProfile);
        }
    }
}
