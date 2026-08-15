using Application.Supplements.Catalog.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Supplements.Catalog.Commands.DeleteSupplementSlot
{
    /// <summary>
    /// Removes a planned dose from the schedule. Doses already taken against it are <b>kept</b>: the handler
    /// clears their <c>ScheduleSlotId</c> first, turning them into ordinary ad-hoc records.
    /// <para>
    /// The FK is <c>Restrict</c> in the database, so relying on it alone would make the delete fail for any
    /// slot the user has ever ticked — i.e. every slot worth deleting. Same mechanism as detaching sessions
    /// from a deleted routine, and materialized transactions from a deleted recurring template.
    /// </para>
    /// </summary>
    public class DeleteSupplementSlotCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<DeleteSupplementSlotCommand, SupplementDto>
    {
        public async Task<SupplementDto> Handle(DeleteSupplementSlotCommand request, CancellationToken cancellationToken)
        {
            var supplement = await unitOfWork.Supplements
                .GetOwnedByIdAsync(request.SupplementId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Supplement with ID {request.SupplementId} not found.");

            var slot = supplement.Slots.FirstOrDefault(s => s.Id == request.SlotId)
                ?? throw new NotFoundException($"Slot with ID {request.SlotId} not found for this supplement.");

            var intakes = await unitOfWork.SupplementIntakes
                .GetForSlotAsync(request.SlotId, cancellationToken)
                .ConfigureAwait(false);

            foreach (var intake in intakes)
                intake.DetachFromSlot();

            supplement.RemoveSlot(slot);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<SupplementDto>(supplement);
        }
    }
}
