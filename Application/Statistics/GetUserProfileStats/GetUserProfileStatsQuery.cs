using MediatR;

namespace Application.Statistics.GetUserProfileStats
{
    public record GetUserProfileStatsQuery(int AccountId) : IRequest<GetUserProfileStatsResponse>;
}
