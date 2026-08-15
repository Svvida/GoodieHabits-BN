using Application.Supplements.Catalog.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Supplements.Catalog.Commands.UpdateSupplementSlot
{
    /// <summary>
    /// Edits a planned dose. Doses already logged against the slot keep the amount they were logged with —
    /// changing the plan is not a retroactive claim about what was swallowed last week.
    /// </summary>
    public class UpdateSupplementSlotCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateSupplementSlotCommand, SupplementDto>
    {
        public async Task<SupplementDto> Handle(UpdateSupplementSlotCommand request, CancellationToken cancellationToken)
        {
            var supplement = await unitOfWork.Supplements
                .GetOwnedByIdAsync(request.SupplementId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Supplement with ID {request.SupplementId} not found.");

            // Resolved inside the loaded supplement, so a slot id from someone else's schedule is simply a 404.
            var slot = supplement.Slots.FirstOrDefault(s => s.Id == request.SlotId)
                ?? throw new NotFoundException($"Slot with ID {request.SlotId} not found for this supplement.");

            slot.Update(request.Timing, request.Amount, request.TimeOfDay, request.OffsetMinutes, request.Note);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<SupplementDto>(supplement);
        }
    }
}
