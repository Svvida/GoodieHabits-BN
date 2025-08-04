using Application.Leaderboard.Dtos;

namespace Application.Leaderboard.GetTopXp
{
    public record GetTopXpResponse(List<LeaderboardItemDto> TopXp);
}
