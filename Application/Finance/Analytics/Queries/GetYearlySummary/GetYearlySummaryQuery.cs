using Application.Common.Interfaces;
using Application.Finance.Analytics.Dtos;

namespace Application.Finance.Analytics.Queries.GetYearlySummary
{
    public record GetYearlySummaryQuery(int UserProfileId, int Year) : IQuery<YearlySummaryDto>;
}
