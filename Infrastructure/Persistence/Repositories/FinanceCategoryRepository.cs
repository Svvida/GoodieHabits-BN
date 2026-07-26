using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class FinanceCategoryRepository(AppDbContext context) : BaseRepository<FinanceCategory>(context), IFinanceCategoryRepository
    {
        // System categories (UserProfileId == null) plus the user's own. Flat list ordered so the
        // application layer can assemble the main -> sub tree (works regardless of tracking).
        public async Task<IEnumerable<FinanceCategory>> GetUserCategoryTreeAsync(int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.FinanceCategories
                .Where(c => c.UserProfileId == userProfileId || c.UserProfileId == null);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query
                .OrderBy(c => c.ParentCategoryId)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<FinanceCategory?> GetOwnedByIdAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.FinanceCategories.Where(c => c.Id == id && c.UserProfileId == userProfileId);
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<FinanceCategory?> GetAssignableByIdAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.FinanceCategories
                .Where(c => c.Id == id && (c.UserProfileId == userProfileId || c.UserProfileId == null));
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<FinanceCategory>> GetOwnedByIdsAsync(IEnumerable<int> ids, int userProfileId, CancellationToken cancellationToken = default)
        {
            var idList = ids.Distinct().ToList();
            if (idList.Count == 0)
                return [];

            return await _context.FinanceCategories
                .Where(c => idList.Contains(c.Id) && c.UserProfileId == userProfileId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<int>> GetSubCategoryIdsAsync(IEnumerable<int> parentIds, CancellationToken cancellationToken = default)
        {
            var parentList = parentIds.Distinct().ToList();
            if (parentList.Count == 0)
                return [];

            return await _context.FinanceCategories
                .AsNoTracking()
                .Where(c => c.ParentCategoryId != null && parentList.Contains(c.ParentCategoryId.Value))
                .Select(c => c.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        // Tracked on purpose: the caller mutates these to cascade an inherited change from the parent.
        public async Task<IReadOnlyList<FinanceCategory>> GetOwnedSubCategoriesAsync(int parentCategoryId, int userProfileId, CancellationToken cancellationToken = default)
        {
            return await _context.FinanceCategories
                .Where(c => c.ParentCategoryId == parentCategoryId && c.UserProfileId == userProfileId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> IsNameUniqueUnderParentAsync(
            string name,
            int userProfileId,
            FinanceTransactionTypeEnum type,
            int? parentCategoryId,
            int? excludeCategoryId,
            CancellationToken cancellationToken = default)
        {
            return !await _context.FinanceCategories
                .AnyAsync(
                    c => c.UserProfileId == userProfileId
                        && c.Type == type
                        && c.ParentCategoryId == parentCategoryId
                        && c.Name == name
                        && (excludeCategoryId == null || c.Id != excludeCategoryId),
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
