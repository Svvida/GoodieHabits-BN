using Application.Common.Interfaces;
using Application.Supplements.Intakes.Dtos;

namespace Application.Supplements.Intakes.Queries.GetIntakes
{
    /// <summary>Raw intake history over an inclusive date range — planned and ad-hoc alike.</summary>
    public record GetIntakesQuery(int UserProfileId, DateOnly From, DateOnly To)
        : IQuery<IEnumerable<SupplementIntakeDto>>;
}
