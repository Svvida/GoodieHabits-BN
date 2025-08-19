using Application.Common.Interfaces;

namespace Application.Statistics.Queries.GetUserExtendedStats
{
    public record GetUserExtendedStatsQuery(int AccountId) : IQuery<GetUserExtendedStatsResponse>;
}
