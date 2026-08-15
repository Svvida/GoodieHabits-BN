using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class WorkoutRoutineRepository(AppDbContext context)
        : BaseRepository<WorkoutRoutine>(context), IWorkoutRoutineRepository
    {
        public async Task<IReadOnlyList<WorkoutRoutine>> GetUserRoutinesAsync(
            int userProfileId, bool includeArchived, CancellationToken cancellationToken = default)
        {
            var query = _context.WorkoutRoutines
                .AsNoTracking()
                .Where(r => r.UserProfileId == userProfileId);

            if (!includeArchived)
                query = query.Where(r => !r.IsArchived);

            return await query
                .Include(r => r.Exercises.OrderBy(e => e.Order))
                    .ThenInclude(e => e.Exercise)
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<WorkoutRoutine?> GetOwnedByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            // The Exercise include is load-bearing, not a convenience: starting a session snapshots each
            // exercise's name and metric, which a partially loaded graph cannot supply.
            var query = _context.WorkoutRoutines
                .Include(r => r.Exercises.OrderBy(e => e.Order))
                    .ThenInclude(e => e.Exercise)
                .Where(r => r.Id == id && r.UserProfileId == userProfileId);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> ExistsByNameAsync(
            int userProfileId, string name, int? excludeId, CancellationToken cancellationToken = default)
        {
            var trimmed = name.Trim();

            return await _context.WorkoutRoutines
                .AsNoTracking()
                .AnyAsync(
                    r => r.UserProfileId == userProfileId
                        && r.Name == trimmed
                        && (excludeId == null || r.Id != excludeId),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
