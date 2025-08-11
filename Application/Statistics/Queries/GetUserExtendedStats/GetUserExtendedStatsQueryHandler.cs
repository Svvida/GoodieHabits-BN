using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Statistics.Queries.GetUserExtendedStats
{
    public class GetUserExtendedStatsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserExtendedStatsQuery, GetUserExtendedStatsResponse>
    {
        public async Task<GetUserExtendedStatsResponse> Handle(GetUserExtendedStatsQuery request, CancellationToken cancellationToken)
        {
            var profile = await unitOfWork.UserProfiles.GetUserProfileWithGoalsAsync(request.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with ID {request.AccountId} not found.");

            return mapper.Map<GetUserExtendedStatsResponse>(profile);
        }
    }
}
