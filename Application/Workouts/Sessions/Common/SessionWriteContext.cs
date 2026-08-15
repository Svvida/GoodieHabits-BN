using Application.Workouts.Sessions.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MapsterMapper;

namespace Application.Workouts.Sessions.Common
{
    /// <summary>
    /// The three steps every session write repeats: load the caller's session with its whole tree, find a
    /// child inside it, and hand the refreshed session back as the response.
    /// <para>
    /// Resolving children <em>within the loaded session</em> is what makes ownership checks free — a set id
    /// from someone else's session simply isn't in the graph, so it reads as a 404 without a second query.
    /// </para>
    /// </summary>
    internal static class SessionWriteContext
    {
        public static async Task<WorkoutSession> LoadAsync(
            IUnitOfWork unitOfWork, int sessionId, int userProfileId, CancellationToken cancellationToken)
        {
            return await unitOfWork.WorkoutSessions
                .GetOwnedByIdAsync(sessionId, userProfileId, false, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Session with ID {sessionId} not found.");
        }

        public static WorkoutSessionExercise FindExercise(WorkoutSession session, int entryId)
            => session.Exercises.FirstOrDefault(e => e.Id == entryId)
               ?? throw new NotFoundException($"Exercise entry with ID {entryId} not found in this session.");

        public static WorkoutSet FindSet(WorkoutSessionExercise entry, int setId)
            => entry.Sets.FirstOrDefault(s => s.Id == setId)
               ?? throw new NotFoundException($"Set with ID {setId} not found in this exercise entry.");

        /// <summary>
        /// Re-reads the session after the write so the response carries server-assigned ids, renumbered
        /// positions and recomputed totals — the same read-back contract the routines slice uses.
        /// </summary>
        public static async Task<WorkoutSessionDto> ReadBackAsync(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            int sessionId,
            int userProfileId,
            CancellationToken cancellationToken)
        {
            var saved = await unitOfWork.WorkoutSessions
                .GetOwnedByIdAsync(sessionId, userProfileId, true, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Session with ID {sessionId} not found.");

            return mapper.Map<WorkoutSessionDto>(saved);
        }
    }
}
