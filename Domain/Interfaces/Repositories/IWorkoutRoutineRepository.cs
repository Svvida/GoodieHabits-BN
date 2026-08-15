using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IWorkoutRoutineRepository : IBaseRepository<WorkoutRoutine>
    {
        Task<IReadOnlyList<WorkoutRoutine>> GetUserRoutinesAsync(
            int userProfileId, bool includeArchived, CancellationToken cancellationToken = default);

        /// <summary>
        /// The routine with its exercises <em>and</em> each item's <c>Exercise</c> loaded. Starting a session
        /// snapshots the exercise name and metric, which cannot be read from a partially loaded graph — so this
        /// include list is load-bearing, not a convenience.
        /// </summary>
        Task<WorkoutRoutine?> GetOwnedByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        Task<bool> ExistsByNameAsync(
            int userProfileId, string name, int? excludeId, CancellationToken cancellationToken = default);
    }
}
