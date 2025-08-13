using Application.Leaderboard.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Leaderboard.Queries.GetTopXp
{
    public class GetTopXpQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTopXpQuery, List<LeaderboardItemDto>>
    {
        public async Task<List<LeaderboardItemDto>> Handle(GetTopXpQuery request, CancellationToken cancellationToken)
        {
            var profiles = await unitOfWork.UserProfiles.GetTenProfilesWithMostXpAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<List<LeaderboardItemDto>>(profiles);
        }
    }
}
