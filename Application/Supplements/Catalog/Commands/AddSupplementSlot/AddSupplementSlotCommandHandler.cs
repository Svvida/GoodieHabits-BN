using Application.Supplements.Catalog.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Supplements.Catalog.Commands.AddSupplementSlot
{
    /// <summary>
    /// Adds a planned dose. Returns the whole supplement so the client repaints its schedule from one response.
    /// <para>
    /// Duplicate timings are deliberately allowed: "two capsules in the morning, recorded as two doses" is a
    /// real way people take supplements, and refusing it would be restriction for its own sake.
    /// </para>
    /// </summary>
    public class AddSupplementSlotCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<AddSupplementSlotCommand, SupplementDto>
    {
        public async Task<SupplementDto> Handle(AddSupplementSlotCommand request, CancellationToken cancellationToken)
        {
            var supplement = await unitOfWork.Supplements
                .GetOwnedByIdAsync(request.SupplementId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Supplement with ID {request.SupplementId} not found.");

            supplement.AddSlot(request.Timing, request.Amount, request.TimeOfDay, request.OffsetMinutes, request.Note);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<SupplementDto>(supplement);
        }
    }
}
