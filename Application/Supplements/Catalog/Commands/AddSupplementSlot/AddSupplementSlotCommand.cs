using Application.Common.Interfaces;
using Application.Supplements.Catalog.Dtos;
using Domain.Enums;

namespace Application.Supplements.Catalog.Commands.AddSupplementSlot
{
    public record AddSupplementSlotCommand(
        int SupplementId,
        SupplementTimingEnum Timing,
        decimal Amount,
        TimeOnly? TimeOfDay,
        int? OffsetMinutes,
        string? Note,
        int UserProfileId) : ICommand<SupplementDto>;
}
