using Application.Workouts.Sessions.Common;
using Application.Workouts.Sessions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;
using NodaTime;

namespace Application.Workouts.Sessions.Commands.AddSessionExercise
{
    public class AddSessionExerciseCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
        : IRequestHandler<AddSessionExerciseCommand, WorkoutSessionDto>
    {
        public async Task<WorkoutSessionDto> Handle(AddSessionExerciseCommand request, CancellationToken cancellationToken)
        {
            var session = await SessionWriteContext
                .LoadAsync(unitOfWork, request.SessionId, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            var input = request.Exercise;

            var exercise = await unitOfWork.Exercises
                .GetVisibleByIdAsync(input.ExerciseId, request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Exercise with ID {input.ExerciseId} not found.");

            // Snapshots the exercise's name and metric — from here on the entry is independent of the library.
            var entry = session.AddExercise(
                exercise,
                input.TargetSets,
                input.TargetReps,
                input.TargetWeight,
                input.TargetDurationSeconds,
                input.TargetDistance,
                input.RestSeconds,
                input.Note);

            var nowUtc = clock.GetCurrentInstant().ToDateTimeUtc();

            foreach (var set in input.Sets ?? [])
            {
                entry.AddSet(
                    set.CompletedAt ?? nowUtc,
                    set.Reps,
                    set.Weight,
                    set.DurationSeconds,
                    set.Distance,
                    set.Rpe,
                    set.SetType);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await SessionWriteContext
                .ReadBackAsync(unitOfWork, mapper, session.Id, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
