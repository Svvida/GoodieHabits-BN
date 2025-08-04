using MediatR;

namespace Application.Leaderboard.GetTopXp
{
    public record GetTopXpQuery() : IRequest<GetTopXpResponse>;
}
