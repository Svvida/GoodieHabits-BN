using Application.Supplements.Intakes.Common;
using Application.Supplements.Intakes.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.Supplements.Intakes.Commands.ToggleIntake
{
    /// <summary>
    /// Ticks a planned dose on or off for a given day. Idempotent in both directions: ticking twice leaves one
    /// intake, un-ticking something that was never ticked is a silent no-op.
    /// <para>
    /// The (slot, date) uniqueness is enforced by a filtered unique index, so two racing taps cannot produce
    /// two doses even if both pass this check — the same structural guarantee
    /// <c>UNIQUE (QuestId, PeriodStart)</c> gives quest occurrences.
    /// </para>
    /// <para>
    /// Returns the whole day's checklist rather than the single row, so the client repaints from one response.
    /// </para>
    /// </summary>
    public class ToggleIntakeCommandHandler(IUnitOfWork unitOfWork, IClock clock)
        : IRequestHandler<ToggleIntakeCommand, SupplementChecklistDto>
    {
        public async Task<SupplementChecklistDto> Handle(ToggleIntakeCommand request, CancellationToken cancellationToken)
        {
            var supplement = await unitOfWork.Supplements
                .GetOwnedByIdAsync(request.SupplementId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Supplement with ID {request.SupplementId} not found.");

            var slot = supplement.Slots.FirstOrDefault(s => s.Id == request.SlotId)
                ?? throw new NotFoundException($"Slot with ID {request.SlotId} not found for this supplement.");

            var existing = await unitOfWork.SupplementIntakes
                .GetBySlotAndDayAsync(request.SlotId, request.Date, cancellationToken)
                .ConfigureAwait(false);

            if (request.Taken)
            {
                if (existing is null)
                {
                    var intake = SupplementIntake.Create(
                        supplement,
                        request.Date,
                        clock.GetCurrentInstant().ToDateTimeUtc(),
                        slot,
                        request.Amount,
                        request.WorkoutSessionId);

                    await unitOfWork.SupplementIntakes.AddAsync(intake, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    // Already ticked: only ever refine it. Re-sending the same payload changes nothing.
                    if (request.Amount is decimal amount)
                        existing.UpdateAmount(amount);

                    if (request.WorkoutSessionId is not null)
                        existing.AttachToSession(request.WorkoutSessionId);
                }
            }
            else if (existing is not null)
            {
                unitOfWork.SupplementIntakes.Remove(existing);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await ChecklistBuilder
                .BuildAsync(unitOfWork, request.UserProfileId, request.Date, null, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
