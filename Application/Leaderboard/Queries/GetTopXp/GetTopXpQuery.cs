using Application.Leaderboard.Dtos;
using MediatR;

namespace Application.Leaderboard.Queries.GetTopXp
{
    public record GetTopXpQuery() : IRequest<List<LeaderboardItemDto>>;
}
