using Application.Common.Interfaces;
using Application.Leaderboard.Dtos;

namespace Application.Leaderboard.Queries.GetTopXp
{
    public record GetTopXpQuery() : IQuery<List<LeaderboardItemDto>>;
}
