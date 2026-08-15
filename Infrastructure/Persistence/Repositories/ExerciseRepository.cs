using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ExerciseRepository(AppDbContext context) : BaseRepository<Exercise>(context), IExerciseRepository
    {
        public async Task<IReadOnlyList<Exercise>> GetVisibleAsync(
            int userProfileId,
            MuscleGroupEnum? muscleGroup,
            ExerciseMetricEnum? metricType,
            string? search,
            bool includeArchived,
            CancellationToken cancellationToken = default)
        {
            // "System rows + mine" — a user never sees another user's exercises.
            var query = _context.Exercises
                .AsNoTracking()
                .Where(e => e.UserProfileId == null || e.UserProfileId == userProfileId);

            if (!includeArchived)
                query = query.Where(e => !e.IsArchived);

            if (muscleGroup.HasValue)
                query = query.Where(e => e.MuscleGroup == muscleGroup.Value);

            if (metricType.HasValue)
                query = query.Where(e => e.MetricType == metricType.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(e => e.Name.Contains(term));
            }

            return await query
                .OrderBy(e => e.MuscleGroup)
                .ThenBy(e => e.Name)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Exercise?> GetVisibleByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.Exercises
                .Where(e => e.Id == id && (e.UserProfileId == null || e.UserProfileId == userProfileId));

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Exercise>> GetVisibleByIdsAsync(
            IEnumerable<int> ids, int userProfileId, CancellationToken cancellationToken = default)
        {
            var idList = ids.Distinct().ToList();
            if (idList.Count == 0)
                return [];

            return await _context.Exercises
                .AsNoTracking()
                .Where(e => idList.Contains(e.Id) && (e.UserProfileId == null || e.UserProfileId == userProfileId))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Exercise?> GetOwnedByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.Exercises.Where(e => e.Id == id && e.UserProfileId == userProfileId);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> ExistsByNameAsync(
            int userProfileId, string name, int? excludeId, CancellationToken cancellationToken = default)
        {
            var trimmed = name.Trim();

            return await _context.Exercises
                .AsNoTracking()
                .AnyAsync(
                    e => e.UserProfileId == userProfileId
                        && e.Name == trimmed
                        && (excludeId == null || e.Id != excludeId),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<string>> GetRoutineNamesUsingAsync(
            int exerciseId, CancellationToken cancellationToken = default)
        {
            return await _context.WorkoutRoutineExercises
                .AsNoTracking()
                .Where(e => e.ExerciseId == exerciseId)
                .Select(e => e.WorkoutRoutine.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<WorkoutSessionExercise>> GetSessionEntriesForExerciseAsync(
            int exerciseId, CancellationToken cancellationToken = default)
        {
            // Tracked on purpose: the delete handler clears the FK on these in the same unit of work.
            return await _context.WorkoutSessionExercises
                .Where(e => e.ExerciseId == exerciseId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
