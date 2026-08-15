using Application.Common.Interfaces;
using Application.Supplements.Catalog.Dtos;
using Domain.Enums;

namespace Application.Supplements.Catalog.Commands.UpdateSupplementSlot
{
    public record UpdateSupplementSlotCommand(
        int SupplementId,
        int SlotId,
        SupplementTimingEnum Timing,
        decimal Amount,
        TimeOnly? TimeOfDay,
        int? OffsetMinutes,
        string? Note,
        int UserProfileId) : ICommand<SupplementDto>;
}
