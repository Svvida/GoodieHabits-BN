using Application.Dtos.Stats;
using Application.Dtos.UserProfileStats;

namespace Application.Interfaces
{
    public interface IStatsService
    {
        Task<GetUserProfileStatsDto> GetUserProfileStatsAsync(int accountId, CancellationToken cancellationToken = default);
        Task<GetUserExtendedStatsDto> GetUserExtendedStatsAsync(int accountId, CancellationToken cancellationToken = default);
    }
}
