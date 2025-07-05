using Application.Dtos.Leaderboard;

namespace Application.Interfaces
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardItemDto>> GetTopXpLeaderboardAsync(CancellationToken cancellationToken = default);
    }
}
