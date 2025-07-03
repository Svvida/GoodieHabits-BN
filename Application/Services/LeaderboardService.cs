using Application.Dtos.Leaderboard;
using Application.Interfaces;
using AutoMapper;
using Domain.Interfaces;

namespace Application.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LeaderboardService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<LeaderboardItemDto>> GetTopXpLeaderboardAsync(CancellationToken cancellationToken = default)
        {
            var profiles = await _unitOfWork.UserProfiles.GetTenProfilesWithMostXpAsync(cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<List<LeaderboardItemDto>>(profiles);
        }
    }
}
