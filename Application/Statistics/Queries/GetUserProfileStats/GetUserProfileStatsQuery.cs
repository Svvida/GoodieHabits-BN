using Application.Dtos.UserProfileStats;
using MediatR;

namespace Application.Statistics.Queries.GetProfileStats
{
    public record GetUserProfileStatsQuery(int AccountId) : IRequest<GetUserProfileStatsDto>;
}
