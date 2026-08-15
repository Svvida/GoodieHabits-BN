using Application.Supplements.Intakes.Common;
using Application.Supplements.Intakes.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.Supplements.Intakes.Commands.LogAdHocIntake
{
    /// <summary>
    /// Records a dose that no slot planned. A model that could only record planned doses would punish the user
    /// for exactly the deviation worth writing down, so this path carries no schedule at all.
    /// <para>
    /// Works for an inactive supplement too: deactivating means "off my plan", not "I can never take this".
    /// </para>
    /// </summary>
    public class LogAdHocIntakeCommandHandler(IUnitOfWork unitOfWork, IClock clock)
        : IRequestHandler<LogAdHocIntakeCommand, SupplementChecklistDto>
    {
        public async Task<SupplementChecklistDto> Handle(LogAdHocIntakeCommand request, CancellationToken cancellationToken)
        {
            var supplement = await unitOfWork.Supplements
                .GetOwnedByIdAsync(request.SupplementId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Supplement with ID {request.SupplementId} not found.");

            // Create falls back to the supplement's default amount, and throws a 400 when there is neither.
            var intake = SupplementIntake.Create(
                supplement,
                request.Date,
                clock.GetCurrentInstant().ToDateTimeUtc(),
                slot: null,
                amount: request.Amount,
                workoutSessionId: request.WorkoutSessionId);

            await unitOfWork.SupplementIntakes.AddAsync(intake, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await ChecklistBuilder
                .BuildAsync(unitOfWork, request.UserProfileId, request.Date, null, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
