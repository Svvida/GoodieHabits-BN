using Application.Workouts.Sessions.Common;
using Application.Workouts.Sessions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;
using NodaTime;

namespace Application.Workouts.Sessions.Commands.AbandonSession
{
    /// <summary>
    /// Gives up on a session without finishing it. The record is kept rather than deleted, so the history stays
    /// honest — and it deliberately raises <b>no</b> completion event: an abandoned session is not an
    /// achievement, and it must never feed gamification.
    /// </summary>
    public class AbandonSessionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
        : IRequestHandler<AbandonSessionCommand, WorkoutSessionDto>
    {
        public async Task<WorkoutSessionDto> Handle(AbandonSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await SessionWriteContext
                .LoadAsync(unitOfWork, request.SessionId, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            session.Abandon(clock.GetCurrentInstant().ToDateTimeUtc());

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await SessionWriteContext
                .ReadBackAsync(unitOfWork, mapper, session.Id, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
