using Application.Common.Interfaces;
using Application.Supplements.Catalog.Dtos;

namespace Application.Supplements.Catalog.Commands.DeleteSupplementSlot
{
    public record DeleteSupplementSlotCommand(int SupplementId, int SlotId, int UserProfileId)
        : ICommand<SupplementDto>;
}
