using Application.Workouts.Routines.Common;
using Application.Workouts.Routines.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Routines.Commands.CreateRoutine
{
    public class CreateRoutineCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<CreateRoutineCommand, WorkoutRoutineDto>
    {
        public async Task<WorkoutRoutineDto> Handle(CreateRoutineCommand request, CancellationToken cancellationToken)
        {
            var nameTaken = await unitOfWork.WorkoutRoutines
                .ExistsByNameAsync(request.UserProfileId, request.Name, null, cancellationToken)
                .ConfigureAwait(false);

            if (nameTaken)
                throw new ConflictException($"A routine named '{request.Name.Trim()}' already exists.");

            var items = await RoutineExerciseBuilder
                .BuildAsync(unitOfWork, request.Exercises, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            var routine = WorkoutRoutine.Create(request.UserProfileId, request.Name, request.Description);
            routine.ReplaceExercises(items);

            await unitOfWork.WorkoutRoutines.AddAsync(routine, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await RoutineResponseBuilder
                .ReadBackAsync(unitOfWork, mapper, routine.Id, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
