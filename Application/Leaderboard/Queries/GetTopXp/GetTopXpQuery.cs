using MediatR;

namespace Application.Leaderboard.Queries.GetTopXp
{
    public record GetTopXpQuery() : IRequest<GetTopXpResponse>;
}
