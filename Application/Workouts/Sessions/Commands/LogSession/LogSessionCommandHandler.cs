using Application.Workouts.Sessions.Common;
using Application.Workouts.Sessions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;
using NodaTime;

namespace Application.Workouts.Sessions.Commands.LogSession
{
    /// <summary>
    /// Bulk logging: replaces the session's whole exercise+set tree in one unit of work.
    /// <para>
    /// The granular endpoints (<c>POST .../sets</c> and friends) and this one write exactly the same rows and
    /// stay interchangeable by design — a client may log live, log offline and sync at the end, or mix the two.
    /// </para>
    /// </summary>
    public class LogSessionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
        : IRequestHandler<LogSessionCommand, WorkoutSessionDto>
    {
        public async Task<WorkoutSessionDto> Handle(LogSessionCommand request, CancellationToken cancellationToken)
        {
            var session = await SessionWriteContext
                .LoadAsync(unitOfWork, request.SessionId, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            var entries = await SessionLogBuilder
                .BuildAsync(
                    unitOfWork,
                    request.Exercises,
                    request.UserProfileId,
                    clock.GetCurrentInstant().ToDateTimeUtc(),
                    cancellationToken)
                .ConfigureAwait(false);

            session.ReplaceLog(entries);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await SessionWriteContext
                .ReadBackAsync(unitOfWork, mapper, session.Id, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
