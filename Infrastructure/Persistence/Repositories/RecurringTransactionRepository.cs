using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class RecurringTransactionRepository(AppDbContext context)
        : BaseRepository<RecurringTransaction>(context), IRecurringTransactionRepository
    {
        public async Task<RecurringTransaction?> GetOwnedByIdAsync(
            int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.RecurringTransactions.Where(r => r.Id == id && r.UserProfileId == userProfileId);
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<RecurringTransaction>> GetUserTemplatesAsync(
            int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.RecurringTransactions.Where(r => r.UserProfileId == userProfileId);
            if (asNoTracking)
                query = query.AsNoTracking();

            return await query
                .OrderByDescending(r => r.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<RecurringTransaction>> GetForMaterializationAsync(
            DateOnly today, CancellationToken cancellationToken = default)
        {
            var currentMonth = new DateOnly(today.Year, today.Month, 1);

            return await _context.RecurringTransactions
                .Where(r => r.IsActive && (r.LastMaterializedOn == null || r.LastMaterializedOn < currentMonth))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        // Paused templates count too: they still point at the category, and the FK is Restrict either way.
        public async Task<bool> AnyForCategoriesAsync(IEnumerable<int> categoryIds, CancellationToken cancellationToken = default)
        {
            var idList = categoryIds.Distinct().ToList();
            if (idList.Count == 0)
                return false;

            return await _context.RecurringTransactions
                .AnyAsync(r => r.CategoryId != null && idList.Contains(r.CategoryId.Value), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
