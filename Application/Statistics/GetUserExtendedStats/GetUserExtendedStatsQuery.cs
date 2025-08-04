using MediatR;

namespace Application.Statistics.GetUserExtendedStats
{
    public record GetUserExtendedStatsQuery(int AccountId) : IRequest<GetUserExtendedStatsResponse>;
}
