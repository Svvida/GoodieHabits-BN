using Application.Workouts.Sessions.Common;
using Application.Workouts.Sessions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Sessions.Commands.RemoveSessionExercise
{
    /// <summary>
    /// Drops an exercise (and its logged sets) from a session, renumbering the remaining positions so the order
    /// stays contiguous.
    /// </summary>
    public class RemoveSessionExerciseCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<RemoveSessionExerciseCommand, WorkoutSessionDto>
    {
        public async Task<WorkoutSessionDto> Handle(RemoveSessionExerciseCommand request, CancellationToken cancellationToken)
        {
            var session = await SessionWriteContext
                .LoadAsync(unitOfWork, request.SessionId, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            var entry = SessionWriteContext.FindExercise(session, request.EntryId);

            session.RemoveExercise(entry);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await SessionWriteContext
                .ReadBackAsync(unitOfWork, mapper, session.Id, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
