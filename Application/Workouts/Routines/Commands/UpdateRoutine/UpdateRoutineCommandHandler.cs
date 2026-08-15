using Application.Workouts.Routines.Common;
using Application.Workouts.Routines.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Routines.Commands.UpdateRoutine
{
    /// <summary>
    /// Full replacement of a routine and its exercise list.
    /// <para>
    /// ⚠️ Editing a routine <b>never</b> reaches sessions already performed from it: a session copied the
    /// exercises and their targets at start, and snapshotted each exercise's name and metric. That is the same
    /// template-versus-record boundary as <c>RecurringTransaction</c> → <c>FinanceTransaction</c>, and there is
    /// a regression test for it.
    /// </para>
    /// </summary>
    public class UpdateRoutineCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateRoutineCommand, WorkoutRoutineDto>
    {
        public async Task<WorkoutRoutineDto> Handle(UpdateRoutineCommand request, CancellationToken cancellationToken)
        {
            var routine = await unitOfWork.WorkoutRoutines
                .GetOwnedByIdAsync(request.RoutineId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Routine with ID {request.RoutineId} not found.");

            var nameTaken = await unitOfWork.WorkoutRoutines
                .ExistsByNameAsync(request.UserProfileId, request.Name, request.RoutineId, cancellationToken)
                .ConfigureAwait(false);

            if (nameTaken)
                throw new ConflictException($"A routine named '{request.Name.Trim()}' already exists.");

            var items = await RoutineExerciseBuilder
                .BuildAsync(unitOfWork, request.Exercises, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            routine.Rename(request.Name);
            routine.UpdateDescription(request.Description);
            routine.ReplaceExercises(items);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await RoutineResponseBuilder
                .ReadBackAsync(unitOfWork, mapper, routine.Id, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
