using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface ISupplementRepository : IBaseRepository<Supplement>
    {
        /// <summary>The catalog with each supplement's schedule slots loaded — the checklist is built from this.</summary>
        Task<IReadOnlyList<Supplement>> GetUserSupplementsAsync(
            int userProfileId, bool includeInactive, CancellationToken cancellationToken = default);

        Task<Supplement?> GetOwnedByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        Task<bool> ExistsByNameAsync(
            int userProfileId, string name, int? excludeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete guard: a supplement with logged intakes cannot be deleted, because that would erase history.
        /// Deactivating is the retire path.
        /// </summary>
        Task<bool> HasIntakesAsync(int supplementId, CancellationToken cancellationToken = default);
    }
}
