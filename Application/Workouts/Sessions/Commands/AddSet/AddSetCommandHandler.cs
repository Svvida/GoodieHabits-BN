using Application.Workouts.Sessions.Common;
using Application.Workouts.Sessions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;
using NodaTime;

namespace Application.Workouts.Sessions.Commands.AddSet
{
    public class AddSetCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
        : IRequestHandler<AddSetCommand, WorkoutSessionDto>
    {
        public async Task<WorkoutSessionDto> Handle(AddSetCommand request, CancellationToken cancellationToken)
        {
            var session = await SessionWriteContext
                .LoadAsync(unitOfWork, request.SessionId, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            var entry = SessionWriteContext.FindExercise(session, request.EntryId);

            // AddSet validates the payload against the entry's snapshotted metric and throws a 400 naming the
            // missing measurement.
            entry.AddSet(
                request.Set.CompletedAt ?? clock.GetCurrentInstant().ToDateTimeUtc(),
                request.Set.Reps,
                request.Set.Weight,
                request.Set.DurationSeconds,
                request.Set.Distance,
                request.Set.Rpe,
                request.Set.SetType);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await SessionWriteContext
                .ReadBackAsync(unitOfWork, mapper, session.Id, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
