using Application.Workouts.Sessions.Common;
using Application.Workouts.Sessions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Workouts.Sessions.Commands.DeleteSet
{
    /// <summary>
    /// Removes a logged set and renumbers the rest, so set numbers stay 1..n with no gaps — the client renders
    /// them directly and a hole would read as lost data.
    /// </summary>
    public class DeleteSetCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<DeleteSetCommand, WorkoutSessionDto>
    {
        public async Task<WorkoutSessionDto> Handle(DeleteSetCommand request, CancellationToken cancellationToken)
        {
            var session = await SessionWriteContext
                .LoadAsync(unitOfWork, request.SessionId, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            var entry = SessionWriteContext.FindExercise(session, request.EntryId);
            var set = SessionWriteContext.FindSet(entry, request.SetId);

            entry.RemoveSet(set);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await SessionWriteContext
                .ReadBackAsync(unitOfWork, mapper, session.Id, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
