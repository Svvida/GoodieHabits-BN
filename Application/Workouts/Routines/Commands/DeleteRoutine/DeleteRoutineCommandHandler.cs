using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Workouts.Routines.Commands.DeleteRoutine
{
    /// <summary>
    /// Deletes a routine. Sessions performed from it are <b>kept</b> — the handler clears their
    /// <c>RoutineId</c> first, in the same unit of work.
    /// <para>
    /// The foreign key stays <c>Restrict</c> in the database on purpose: relying on it alone would make the
    /// delete fail outright for any routine old enough to have been used, i.e. every routine worth deleting.
    /// Identical reasoning, and identical mechanism, to deleting a recurring finance template.
    /// </para>
    /// </summary>
    public class DeleteRoutineCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteRoutineCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteRoutineCommand request, CancellationToken cancellationToken)
        {
            var routine = await unitOfWork.WorkoutRoutines
                .GetOwnedByIdAsync(request.RoutineId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Routine with ID {request.RoutineId} not found.");

            var sessions = await unitOfWork.WorkoutSessions
                .GetForRoutineAsync(request.RoutineId, cancellationToken)
                .ConfigureAwait(false);

            foreach (var session in sessions)
                session.DetachRoutine();

            unitOfWork.WorkoutRoutines.Remove(routine);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
