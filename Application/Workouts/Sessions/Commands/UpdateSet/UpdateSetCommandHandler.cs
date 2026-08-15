using Application.Workouts.Sessions.Common;
using Application.Workouts.Sessions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Sessions.Commands.UpdateSet
{
    public class UpdateSetCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<UpdateSetCommand, WorkoutSessionDto>
    {
        public async Task<WorkoutSessionDto> Handle(UpdateSetCommand request, CancellationToken cancellationToken)
        {
            var session = await SessionWriteContext
                .LoadAsync(unitOfWork, request.SessionId, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            var entry = SessionWriteContext.FindExercise(session, request.EntryId);
            var set = SessionWriteContext.FindSet(entry, request.SetId);

            // Revalidated against the entry's metric, exactly like the original write.
            set.Update(
                entry.MetricType,
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
