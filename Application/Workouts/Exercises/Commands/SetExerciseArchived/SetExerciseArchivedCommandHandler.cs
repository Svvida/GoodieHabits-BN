using Application.Workouts.Exercises.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Exercises.Commands.SetExerciseArchived
{
    /// <summary>
    /// The retire path: hides an exercise from the picker while leaving it in every routine and past session
    /// that already references it. This is what a user reaches for when delete is blocked.
    /// <para>
    /// System exercises cannot be archived — that would need per-user state on a shared row. Hiding unwanted
    /// system exercises is a deliberate backlog item, not an oversight.
    /// </para>
    /// </summary>
    public class SetExerciseArchivedCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<SetExerciseArchivedCommand, ExerciseDto>
    {
        public async Task<ExerciseDto> Handle(SetExerciseArchivedCommand request, CancellationToken cancellationToken)
        {
            var exercise = await unitOfWork.Exercises
                .GetVisibleByIdAsync(request.ExerciseId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Exercise with ID {request.ExerciseId} not found.");

            if (exercise.IsSystem)
                throw new ForbiddenException("System exercises cannot be archived.");

            exercise.SetArchived(request.IsArchived);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return mapper.Map<ExerciseDto>(exercise);
        }
    }
}
