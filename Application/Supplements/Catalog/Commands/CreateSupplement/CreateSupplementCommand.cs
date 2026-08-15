using Application.Common.Interfaces;
using Application.Supplements.Catalog.Dtos;
using Domain.Enums;

namespace Application.Supplements.Catalog.Commands.CreateSupplement
{
    public record CreateSupplementCommand(
        string Name,
        SupplementUnitEnum Unit,
        decimal? DefaultAmount,
        string? Note,
        string? Color,
        string? Icon,
        int UserProfileId) : ICommand<SupplementDto>;
}
