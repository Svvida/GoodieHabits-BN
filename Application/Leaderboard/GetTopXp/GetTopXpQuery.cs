using Application.Dtos.Leaderboard;
using MediatR;

namespace Application.Leaderboard.GetTopXp
{
    public record GetTopXpQuery() : IRequest<List<LeaderboardItemDto>>;
}
