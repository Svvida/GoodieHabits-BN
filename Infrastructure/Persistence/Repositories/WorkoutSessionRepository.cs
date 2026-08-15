using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class WorkoutSessionRepository(AppDbContext context)
        : BaseRepository<WorkoutSession>(context), IWorkoutSessionRepository
    {
        public async Task<WorkoutSession?> GetActiveAsync(
            int userProfileId, CancellationToken cancellationToken = default)
        {
            return await WithTree(_context.WorkoutSessions)
                .FirstOrDefaultAsync(
                    s => s.UserProfileId == userProfileId && s.Status == WorkoutSessionStatusEnum.InProgress,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<WorkoutSession?> GetOwnedByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = WithTree(_context.WorkoutSessions)
                .Where(s => s.Id == id && s.UserProfileId == userProfileId);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<(IReadOnlyList<WorkoutSession> Items, int TotalCount)> GetUserSessionsAsync(
            int userProfileId,
            DateOnly? from,
            DateOnly? to,
            WorkoutSessionStatusEnum? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.WorkoutSessions
                .AsNoTracking()
                .Where(s => s.UserProfileId == userProfileId);

            if (from.HasValue)
                query = query.Where(s => s.PerformedOn >= from.Value);
            if (to.HasValue)
                query = query.Where(s => s.PerformedOn <= to.Value);
            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            var items = await WithTree(query)
                .OrderByDescending(s => s.PerformedOn)
                .ThenByDescending(s => s.StartedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return (items, totalCount);
        }

        public async Task<IReadOnlyList<WorkoutSession>> GetForPeriodAsync(
            int userProfileId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        {
            return await WithTree(_context.WorkoutSessions.AsNoTracking())
                .Where(s => s.UserProfileId == userProfileId && s.PerformedOn >= from && s.PerformedOn <= to)
                .OrderBy(s => s.PerformedOn)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<WorkoutSession>> GetForRoutineAsync(
            int routineId, CancellationToken cancellationToken = default)
        {
            // Tracked: the routine delete handler clears RoutineId on these in the same unit of work.
            return await _context.WorkoutSessions
                .Where(s => s.RoutineId == routineId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<WorkoutSession>> GetForExerciseAsync(
            int userProfileId, int exerciseId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
        {
            return await WithTree(_context.WorkoutSessions.AsNoTracking())
                .Where(s => s.UserProfileId == userProfileId
                    && s.PerformedOn >= from
                    && s.PerformedOn <= to
                    && s.Exercises.Any(e => e.ExerciseId == exerciseId))
                .OrderBy(s => s.PerformedOn)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<ExerciseBest>> GetPersonalRecordsAsync(
            int userProfileId, CancellationToken cancellationToken = default)
        {
            // A grouped projection, so a fold over the user's entire history never materializes set rows.
            // Warm-ups are excluded; entries whose library exercise has been deleted drop out with it.
            var rows = from set in _context.WorkoutSets.AsNoTracking()
                       join entry in _context.WorkoutSessionExercises.AsNoTracking()
                           on set.WorkoutSessionExerciseId equals entry.Id
                       join session in _context.WorkoutSessions.AsNoTracking()
                           on entry.WorkoutSessionId equals session.Id
                       where session.UserProfileId == userProfileId
                           && entry.ExerciseId != null
                           && set.SetType != WorkoutSetTypeEnum.Warmup
                       select new
                       {
                           ExerciseId = entry.ExerciseId!.Value,
                           entry.ExerciseName,
                           set.Reps,
                           set.Weight,
                           session.PerformedOn,
                       };

            return await rows
                .GroupBy(r => r.ExerciseId)
                .Select(g => new ExerciseBest(
                    g.Key,
                    // Plain MAX, not "the most recent snapshot": an ordered sub-select inside a grouped
                    // projection does not translate to SQL, and this is precisely the kind of query the
                    // InMemory provider would let through while SQL Server threw. Snapshots of one exercise
                    // are the same string in practice, so the aggregate costs nothing.
                    g.Max(r => r.ExerciseName),
                    g.Max(r => r.Weight),
                    g.Max(r => r.Reps),
                    g.Max(r => (r.Reps ?? 0) * (r.Weight ?? 0m)),
                    g.Count(),
                    g.Max(r => r.PerformedOn)))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Every read path except the detach query needs the full exercise+set tree — the session is an
        /// aggregate and its totals are folded in memory over these rows.
        /// </summary>
        private static IQueryable<WorkoutSession> WithTree(IQueryable<WorkoutSession> query)
            => query
                .Include(s => s.Exercises.OrderBy(e => e.Order))
                    .ThenInclude(e => e.Sets.OrderBy(x => x.SetNumber));
    }
}
