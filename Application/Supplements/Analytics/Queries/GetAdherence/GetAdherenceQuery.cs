using Application.Common.Interfaces;
using Application.Supplements.Analytics.Dtos;

namespace Application.Supplements.Analytics.Queries.GetAdherence
{
    public record GetAdherenceQuery(int UserProfileId, DateOnly From, DateOnly To)
        : IQuery<SupplementAdherenceReportDto>;
}
