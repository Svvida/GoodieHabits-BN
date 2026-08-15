using Application.Common.Interfaces;
using Application.Supplements.Catalog.Dtos;
using Domain.Enums;

namespace Application.Supplements.Catalog.Commands.UpdateSupplement
{
    public record UpdateSupplementCommand(
        int SupplementId,
        string Name,
        SupplementUnitEnum Unit,
        decimal? DefaultAmount,
        string? Note,
        string? Color,
        string? Icon,
        int UserProfileId) : ICommand<SupplementDto>;
}
