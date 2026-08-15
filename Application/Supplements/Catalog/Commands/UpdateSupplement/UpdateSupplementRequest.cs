using Domain.Enums;

namespace Application.Supplements.Catalog.Commands.UpdateSupplement
{
    // Full replacement of the supplement's own fields. Slots are managed through their own endpoints, so an
    // edit here can never silently wipe a schedule. Activation is PATCH /{id}/active.
    public record UpdateSupplementRequest(
        string Name,
        SupplementUnitEnum Unit,
        decimal? DefaultAmount = null,
        string? Note = null,
        string? Color = null,
        string? Icon = null);
}
