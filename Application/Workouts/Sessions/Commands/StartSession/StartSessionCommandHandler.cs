using Application.Workouts.Sessions.Common;
using Application.Workouts.Sessions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;
using MediatR;
using NodaTime;

namespace Application.Workouts.Sessions.Commands.StartSession
{
    /// <summary>
    /// Opens a session, either from a saved routine or ad hoc.
    /// <para>
    /// At most one session per user may be in progress. The database enforces it with a filtered unique index;
    /// this check exists to turn that into a message the client can act on, and it names the id so the client
    /// can offer "resume" without a second round-trip.
    /// </para>
    /// <para>
    /// Starting from a routine <b>copies</b> its exercises, targets, names and metrics. The session is a record
    /// from that moment on and later edits to the routine never reach it.
    /// </para>
    /// </summary>
    public class StartSessionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IClock clock)
        : IRequestHandler<StartSessionCommand, WorkoutSessionDto>
    {
        public async Task<WorkoutSessionDto> Handle(StartSessionCommand request, CancellationToken cancellationToken)
        {
            var active = await unitOfWork.WorkoutSessions
                .GetActiveAsync(request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);

            if (active is not null)
                throw new ConflictException(
                    $"A session is already in progress (id {active.Id}). Finish or abandon it first.");

            var nowUtc = clock.GetCurrentInstant().ToDateTimeUtc();
            var performedOn = request.PerformedOn ?? await ResolveLocalTodayAsync(request.UserProfileId, nowUtc, cancellationToken).ConfigureAwait(false);

            WorkoutSession session;

            if (request.RoutineId is int routineId)
            {
                var routine = await unitOfWork.WorkoutRoutines
                    .GetOwnedByIdAsync(routineId, request.UserProfileId, false, cancellationToken)
                    .ConfigureAwait(false)
                    ?? throw new NotFoundException($"Routine with ID {routineId} not found.");

                session = WorkoutSession.StartFromRoutine(
                    request.UserProfileId, routine, performedOn, nowUtc, request.Note);

                if (!string.IsNullOrWhiteSpace(request.Name))
                    session.Rename(request.Name);
            }
            else
            {
                session = WorkoutSession.Start(
                    request.UserProfileId, request.Name!, performedOn, nowUtc, request.Note);
            }

            await unitOfWork.WorkoutSessions.AddAsync(session, cancellationToken).ConfigureAwait(false);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return await SessionWriteContext
                .ReadBackAsync(unitOfWork, mapper, session.Id, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// "Which day is it for this user" goes through <see cref="UserProfile.LocalDateOn"/> — the single
        /// source of truth for that question in the codebase (ARCHITECTURE §6). Deriving it from UTC here
        /// would put a session on the wrong day for anyone east or west of the server.
        /// </summary>
        private async Task<DateOnly> ResolveLocalTodayAsync(
            int userProfileId, DateTime nowUtc, CancellationToken cancellationToken)
        {
            var profile = await unitOfWork.UserProfiles
                .GetByIdAsync(userProfileId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with ID {userProfileId} not found.");

            return profile.LocalDateOn(nowUtc);
        }
    }
}
