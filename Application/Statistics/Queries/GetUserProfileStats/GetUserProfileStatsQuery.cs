using Application.Common.Interfaces;

namespace Application.Statistics.Queries.GetUserProfileStats
{
    public record GetUserProfileStatsQuery(int AccountId) : IQuery<GetUserProfileStatsResponse>;
}
