using Application.Dtos.Stats;
using MediatR;

namespace Application.Statistics.Queries.GetUserExtendedStats
{
    public record GetUserExtendedStatsQuery(int AccountId) : IRequest<GetUserExtendedStatsDto>;
}
