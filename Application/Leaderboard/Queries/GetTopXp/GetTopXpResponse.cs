using Application.Leaderboard.Dtos;

namespace Application.Leaderboard.Queries.GetTopXp
{
    public record GetTopXpResponse(List<LeaderboardItemDto> TopXp);
}
