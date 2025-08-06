using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Statistics.GetUserExtendedStats
{
    public class GetUserExtendedStatsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserExtendedStatsQuery, GetUserExtendedStatsResponse>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        public async Task<GetUserExtendedStatsResponse> Handle(GetUserExtendedStatsQuery request, CancellationToken cancellationToken)
        {
            var profile = await _unitOfWork.UserProfiles.GetUserProfileWithGoalsAsync(request.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with ID {request.AccountId} not found.");

            return _mapper.Map<GetUserExtendedStatsResponse>(profile);
        }
    }
}
