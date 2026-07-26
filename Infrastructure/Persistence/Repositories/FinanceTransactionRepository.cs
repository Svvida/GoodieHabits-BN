using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class FinanceTransactionRepository(AppDbContext context) : BaseRepository<FinanceTransaction>(context), IFinanceTransactionRepository
    {
        public async Task<FinanceTransaction?> GetOwnedByIdAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.FinanceTransactions.Where(t => t.Id == id && t.UserProfileId == userProfileId);
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<(IReadOnlyList<FinanceTransaction> Items, int TotalCount)> GetUserTransactionsAsync(
            int userProfileId,
            DateOnly? from,
            DateOnly? to,
            FinanceTransactionTypeEnum? type,
            int? categoryId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.FinanceTransactions
                .AsNoTracking()
                .Where(t => t.UserProfileId == userProfileId);

            if (from.HasValue)
                query = query.Where(t => t.OccurredOn >= from.Value);
            if (to.HasValue)
                query = query.Where(t => t.OccurredOn <= to.Value);
            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);
            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var items = await query
                .OrderByDescending(t => t.OccurredOn)
                .ThenByDescending(t => t.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return (items, totalCount);
        }

        public async Task<IReadOnlyList<FinanceTransaction>> GetForPeriodAsync(
            int userProfileId,
            DateOnly from,
            DateOnly to,
            FinanceTransactionTypeEnum? type,
            CancellationToken cancellationToken = default)
        {
            var query = _context.FinanceTransactions
                .AsNoTracking()
                .Where(t => t.UserProfileId == userProfileId && t.OccurredOn >= from && t.OccurredOn <= to);

            if (type.HasValue)
                query = query.Where(t => t.Type == type.Value);

            return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> AnyForCategoriesAsync(IEnumerable<int> categoryIds, CancellationToken cancellationToken = default)
        {
            var idList = categoryIds.Distinct().ToList();
            if (idList.Count == 0)
                return false;

            return await _context.FinanceTransactions
                .AnyAsync(t => t.CategoryId != null && idList.Contains(t.CategoryId.Value), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
