using Application.Dtos.Leaderboard;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Leaderboard.GetTopXp
{
    public class GetTopXpQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetTopXpQuery, List<LeaderboardItemDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<List<LeaderboardItemDto>> Handle(GetTopXpQuery request, CancellationToken cancellationToken)
        {
            var profiles = await _unitOfWork.UserProfiles.GetTenProfilesWithMostXpAsync(cancellationToken).ConfigureAwait(false);

            return _mapper.Map<List<LeaderboardItemDto>>(profiles);
        }
    }
}
