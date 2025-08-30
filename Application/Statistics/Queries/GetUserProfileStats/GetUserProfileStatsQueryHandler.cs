using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Statistics.Queries.GetUserProfileStats
{
    public class GetUserProfileStatsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetUserProfileStatsQuery, GetUserProfileStatsResponse>
    {
        public async Task<GetUserProfileStatsResponse> Handle(GetUserProfileStatsQuery request, CancellationToken cancellationToken)
        {
            var profile = await unitOfWork.UserProfiles.GetUserProfileWithGoalsAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with ID {request.UserProfileId} not found.");

            return mapper.Map<GetUserProfileStatsResponse>(profile);
        }
    }
}
