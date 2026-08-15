using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class SupplementRepository(AppDbContext context)
        : BaseRepository<Supplement>(context), ISupplementRepository
    {
        public async Task<IReadOnlyList<Supplement>> GetUserSupplementsAsync(
            int userProfileId, bool includeInactive, CancellationToken cancellationToken = default)
        {
            var query = _context.Supplements
                .AsNoTracking()
                .Where(s => s.UserProfileId == userProfileId);

            if (!includeInactive)
                query = query.Where(s => s.IsActive);

            return await query
                .Include(s => s.Slots.OrderBy(slot => slot.Timing).ThenBy(slot => slot.TimeOfDay))
                .OrderBy(s => s.Name)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<Supplement?> GetOwnedByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.Supplements
                .Include(s => s.Slots.OrderBy(slot => slot.Timing).ThenBy(slot => slot.TimeOfDay))
                .Where(s => s.Id == id && s.UserProfileId == userProfileId);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> ExistsByNameAsync(
            int userProfileId, string name, int? excludeId, CancellationToken cancellationToken = default)
        {
            var trimmed = name.Trim();

            return await _context.Supplements
                .AsNoTracking()
                .AnyAsync(
                    s => s.UserProfileId == userProfileId
                        && s.Name == trimmed
                        && (excludeId == null || s.Id != excludeId),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> HasIntakesAsync(int supplementId, CancellationToken cancellationToken = default)
        {
            return await _context.SupplementIntakes
                .AsNoTracking()
                .AnyAsync(i => i.SupplementId == supplementId, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
