using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Statistics.GetUserProfileStats
{
    public class GetUserProfileStatsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserProfileStatsQuery, GetUserProfileStatsResponse>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        public async Task<GetUserProfileStatsResponse> Handle(GetUserProfileStatsQuery request, CancellationToken cancellationToken)
        {
            var profile = await _unitOfWork.UserProfiles.GetUserProfileWithGoalsAsync(request.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with ID {request.AccountId} not found.");

            return _mapper.Map<GetUserProfileStatsResponse>(profile);
        }
    }
}
