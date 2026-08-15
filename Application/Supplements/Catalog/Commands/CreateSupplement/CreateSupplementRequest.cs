using Domain.Enums;

namespace Application.Supplements.Catalog.Commands.CreateSupplement
{
    // The unit belongs to the supplement and every one of its slots is dosed in it. Slots are added afterwards
    // through POST /supplements/{id}/slots — a supplement with none is legal, it just never appears on the
    // checklist and is only ever taken ad hoc.
    public record CreateSupplementRequest(
        string Name,
        SupplementUnitEnum Unit,
        decimal? DefaultAmount = null,
        string? Note = null,
        string? Color = null,
        string? Icon = null);
}
