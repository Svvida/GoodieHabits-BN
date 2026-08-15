using Application.Common.Interfaces;
using Application.Supplements.Catalog.Dtos;

namespace Application.Supplements.Catalog.Queries.GetSupplements
{
    public record GetSupplementsQuery(int UserProfileId, bool IncludeInactive)
        : IQuery<IEnumerable<SupplementDto>>;
}
