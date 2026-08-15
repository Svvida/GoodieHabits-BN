using Application.Workouts.Exercises.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Exercises.Commands.CreateExercise
{
    public class CreateExerciseCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<CreateExerciseCommand, ExerciseDto>
    {
        public async Task<ExerciseDto> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
        {
            // Uniqueness is checked here rather than with a database constraint: a unique index would also
            // block reusing the name of an *archived* exercise, which is a legitimate thing to do.
            // Deliberately scoped to the user's own rows — shadowing a system exercise's name is allowed.
            var exists = await unitOfWork.Exercises
                .ExistsByNameAsync(request.UserProfileId, request.Name, null, cancellationToken)
                .ConfigureAwait(false);

            if (exists)
                throw new ConflictException($"An exercise named '{request.Name.Trim()}' already exists.");

            var exercise = Exercise.Create(
                request.UserProfileId,
                request.Name,
                request.MetricType,
                request.MuscleGroup,
                request.Equipment,
                request.Note);

            await unitOfWork.Exercises.AddAsync(exercise, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<ExerciseDto>(exercise);
        }
    }
}
