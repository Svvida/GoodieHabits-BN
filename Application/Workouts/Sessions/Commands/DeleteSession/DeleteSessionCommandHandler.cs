using Application.Workouts.Sessions.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Workouts.Sessions.Commands.DeleteSession
{
    /// <summary>
    /// Hard delete. Exercise entries and their sets go with it (cascade), because they only exist as part of
    /// the session. Supplement intakes taken during the session survive with their link cleared — the FK is
    /// <c>SetNull</c>: the dose was still taken, whatever happened to the training log.
    /// </summary>
    public class DeleteSessionCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteSessionCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await SessionWriteContext
                .LoadAsync(unitOfWork, request.SessionId, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            unitOfWork.WorkoutSessions.Remove(session);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
