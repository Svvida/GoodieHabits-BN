using Application.Supplements.Intakes.Common;
using Application.Supplements.Intakes.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Supplements.Intakes.Commands.DeleteIntake
{
    /// <summary>
    /// Removes a logged dose by id — the undo for an ad-hoc intake, which the checkbox toggle cannot express
    /// (an unplanned dose has no slot to un-tick). Works for a planned dose too.
    /// <para>Returns the checklist for that dose's own date, which is the day the client is looking at.</para>
    /// </summary>
    public class DeleteIntakeCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteIntakeCommand, SupplementChecklistDto>
    {
        public async Task<SupplementChecklistDto> Handle(DeleteIntakeCommand request, CancellationToken cancellationToken)
        {
            var intake = await unitOfWork.SupplementIntakes
                .GetByIdAsync(request.IntakeId, cancellationToken)
                .ConfigureAwait(false);

            // Ownership is checked here rather than in the query so that another user's id reads as "not found"
            // instead of leaking that it exists.
            if (intake is null || intake.UserProfileId != request.UserProfileId)
                throw new NotFoundException($"Intake with ID {request.IntakeId} not found.");

            var date = intake.TakenOn;

            unitOfWork.SupplementIntakes.Remove(intake);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await ChecklistBuilder
                .BuildAsync(unitOfWork, request.UserProfileId, date, null, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
