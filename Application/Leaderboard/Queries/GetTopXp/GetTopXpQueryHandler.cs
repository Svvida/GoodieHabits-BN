using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Leaderboard.Queries.GetTopXp
{
    public class GetTopXpQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTopXpQuery, GetTopXpResponse>
    {
        public async Task<GetTopXpResponse> Handle(GetTopXpQuery request, CancellationToken cancellationToken)
        {
            var profiles = await unitOfWork.UserProfiles.GetTenProfilesWithMostXpAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<GetTopXpResponse>(profiles);
        }
    }
}
