using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IWorkoutSessionRepository : IBaseRepository<WorkoutSession>
    {
        /// <summary>
        /// The user's in-progress session, if any. At most one can exist — a filtered unique index enforces it
        /// — so the start handler uses this to return a friendly conflict carrying the session to resume.
        /// </summary>
        Task<WorkoutSession?> GetActiveAsync(int userProfileId, CancellationToken cancellationToken = default);

        /// <summary>The full tree: exercises and their sets. Every write path needs it.</summary>
        Task<WorkoutSession?> GetOwnedByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<WorkoutSession> Items, int TotalCount)> GetUserSessionsAsync(
            int userProfileId,
            DateOnly? from,
            DateOnly? to,
            WorkoutSessionStatusEnum? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Every session in the inclusive date range with its full tree, for analytics. Aggregation happens in
        /// memory over this, the way all five finance analytics handlers work.
        /// </summary>
        Task<IReadOnlyList<WorkoutSession>> GetForPeriodAsync(
            int userProfileId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tracked sessions produced from a routine, so the routine delete handler can clear their FK first.
        /// A session is the user's own record and must outlive the template it came from — the same rule, and
        /// the same <c>Restrict</c> + null-it-in-the-handler mechanism, as recurring finance templates.
        /// </summary>
        Task<IReadOnlyList<WorkoutSession>> GetForRoutineAsync(
            int routineId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sessions in the range that contain a given library exercise, with the full tree, for the
        /// per-exercise progress chart. Folded in memory — that is where the estimated one-rep max is computed,
        /// because the Epley special cases don't belong in SQL.
        /// </summary>
        Task<IReadOnlyList<WorkoutSession>> GetForExerciseAsync(
            int userProfileId, int exerciseId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

        /// <summary>
        /// Per-exercise maxima over the user's whole history, as a grouped SQL projection — the
        /// <c>MonthlyTotal</c> pattern. **Do not** load set rows and fold this in memory; it is a fold over all
        /// history, not a range.
        /// </summary>
        Task<IReadOnlyList<ExerciseBest>> GetPersonalRecordsAsync(
            int userProfileId, CancellationToken cancellationToken = default);
    }
}
