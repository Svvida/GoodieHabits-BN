using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IExerciseRepository : IBaseRepository<Exercise>
    {
        /// <summary>
        /// The library as one user sees it: seeded system rows plus their own. Mirrors
        /// <c>IFinanceCategoryRepository</c>'s "system + own" read — a user never sees another user's exercises.
        /// </summary>
        Task<IReadOnlyList<Exercise>> GetVisibleAsync(
            int userProfileId,
            MuscleGroupEnum? muscleGroup,
            ExerciseMetricEnum? metricType,
            EquipmentEnum? equipment,
            string? search,
            bool includeArchived,
            CancellationToken cancellationToken = default);

        /// <summary>A system row or one of the user's own — what routine/session writes must resolve against.</summary>
        Task<Exercise?> GetVisibleByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>Bulk form of <see cref="GetVisibleByIdAsync"/>, for validating a whole routine in one query.</summary>
        Task<IReadOnlyList<Exercise>> GetVisibleByIdsAsync(
            IEnumerable<int> ids, int userProfileId, CancellationToken cancellationToken = default);

        /// <summary>Strictly the user's own row — the only thing they may edit, archive or delete.</summary>
        Task<Exercise?> GetOwnedByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        Task<bool> ExistsByNameAsync(
            int userProfileId, string name, int? excludeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Names of the routines still referencing this exercise. Delete is blocked while any exist, and the
        /// handler names them in the conflict — the finance "category still in use" posture.
        /// </summary>
        Task<IReadOnlyList<string>> GetRoutineNamesUsingAsync(
            int exerciseId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tracked session entries pointing at this exercise, so the delete handler can clear their FK. History
        /// keeps the snapshotted name, so deleting an exercise must never take past sessions with it.
        /// </summary>
        Task<IReadOnlyList<WorkoutSessionExercise>> GetSessionEntriesForExerciseAsync(
            int exerciseId, CancellationToken cancellationToken = default);
    }
}
