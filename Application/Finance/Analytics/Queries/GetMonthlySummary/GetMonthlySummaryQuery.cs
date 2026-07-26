using Application.Common.Interfaces;
using Application.Finance.Analytics.Dtos;

namespace Application.Finance.Analytics.Queries.GetMonthlySummary
{
    public record GetMonthlySummaryQuery(int UserProfileId, int Year, int Month) : IQuery<MonthlySummaryDto>;
}
