using Application.Dtos.Stats;
using Application.Dtos.UserProfileStats;
using Application.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Services
{
    public class StatsService : IStatsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public StatsService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GetUserProfileStatsDto> GetUserProfileStatsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var userProfile = await _unitOfWork.UserProfiles.GetUserProfileWithGoalsAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile for account ID {accountId} was not found.");

            return _mapper.Map<GetUserProfileStatsDto>(userProfile);
        }

        public async Task<GetUserExtendedStatsDto> GetUserExtendedStatsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var userProfile = await _unitOfWork.UserProfiles.GetUserProfileWithGoalsAsync(accountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile for account ID {accountId} was not found.");

            return _mapper.Map<GetUserExtendedStatsDto>(userProfile);
        }
    }
}