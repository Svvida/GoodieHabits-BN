using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Workouts.Exercises.Commands.DeleteExercise
{
    /// <summary>
    /// Deletes one of the user's own exercises.
    /// <para>
    /// Blocked while any <b>routine</b> still plans it — the conflict names the offending routines, the same
    /// posture as deleting a finance category that still has transactions. It is deliberately <b>not</b>
    /// blocked by session history: a performed session snapshotted the exercise's name and metric, so the
    /// handler simply clears the foreign key on those entries and the history survives intact.
    /// </para>
    /// <para>Everything happens in one unit of work, so a failure anywhere leaves the library untouched.</para>
    /// </summary>
    public class DeleteExerciseCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteExerciseCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
        {
            var exercise = await unitOfWork.Exercises
                .GetVisibleByIdAsync(request.ExerciseId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Exercise with ID {request.ExerciseId} not found.");

            if (exercise.IsSystem)
                throw new ForbiddenException("System exercises cannot be deleted. Archive your own instead.");

            var routineNames = await unitOfWork.Exercises
                .GetRoutineNamesUsingAsync(request.ExerciseId, cancellationToken)
                .ConfigureAwait(false);

            if (routineNames.Count > 0)
                throw new ConflictException(
                    $"This exercise is still used by: {string.Join(", ", routineNames)}. " +
                    "Remove it from those routines first, or archive it instead.");

            // History keeps the snapshotted name and metric, so past sessions stay readable without the row.
            var sessionEntries = await unitOfWork.Exercises
                .GetSessionEntriesForExerciseAsync(request.ExerciseId, cancellationToken)
                .ConfigureAwait(false);

            foreach (var entry in sessionEntries)
                entry.DetachExercise();

            unitOfWork.Exercises.Remove(exercise);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
