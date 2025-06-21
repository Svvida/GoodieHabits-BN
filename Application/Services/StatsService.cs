using Application.Dtos.UserProfileStats;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services
{
    public class StatsService : IStatsService
    {
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IMapper _mapper;

        public StatsService(IUserProfileRepository userProfileRepository, IMapper mapper)
        {
            _userProfileRepository = userProfileRepository;
            _mapper = mapper;
        }

        public async Task<GetUserProfileStatsDto> GetUserProfileStatsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var userProfile = await _userProfileRepository.GetUserProfileWithGoalsAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile for account ID {accountId} was not found.");

            return _mapper.Map<GetUserProfileStatsDto>(userProfile);
        }
    }
}