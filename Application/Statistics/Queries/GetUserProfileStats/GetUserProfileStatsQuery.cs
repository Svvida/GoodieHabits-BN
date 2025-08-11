using MediatR;

namespace Application.Statistics.Queries.GetUserProfileStats
{
    public record GetUserProfileStatsQuery(int AccountId) : IRequest<GetUserProfileStatsResponse>;
}
