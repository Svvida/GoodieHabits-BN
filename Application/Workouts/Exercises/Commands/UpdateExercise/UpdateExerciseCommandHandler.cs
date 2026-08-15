using Application.Workouts.Exercises.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Exercises.Commands.UpdateExercise
{
    public class UpdateExerciseCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateExerciseCommand, ExerciseDto>
    {
        public async Task<ExerciseDto> Handle(UpdateExerciseCommand request, CancellationToken cancellationToken)
        {
            // Resolved as "visible" rather than "owned" so that editing a system exercise reports what is
            // actually wrong (403) instead of pretending the row does not exist (404).
            var exercise = await unitOfWork.Exercises
                .GetVisibleByIdAsync(request.ExerciseId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Exercise with ID {request.ExerciseId} not found.");

            if (exercise.IsSystem)
                throw new ForbiddenException("System exercises cannot be modified. Create your own instead.");

            var nameTaken = await unitOfWork.Exercises
                .ExistsByNameAsync(request.UserProfileId, request.Name, request.ExerciseId, cancellationToken)
                .ConfigureAwait(false);

            if (nameTaken)
                throw new ConflictException($"An exercise named '{request.Name.Trim()}' already exists.");

            exercise.Rename(request.Name);
            exercise.ChangeMetricType(request.MetricType);
            exercise.UpdateClassification(request.MuscleGroup, request.Equipment);
            exercise.UpdateNote(request.Note);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<ExerciseDto>(exercise);
        }
    }
}
