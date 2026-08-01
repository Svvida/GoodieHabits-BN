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

        public async Task<FinanceTransaction?> GetOwnedWithCorrectionsAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.FinanceTransactions
                .Include(t => t.Corrections)
                .Where(t => t.Id == id && t.UserProfileId == userProfileId);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<FinanceTransaction>> GetCorrectionsForParentsAsync(
            IEnumerable<int> parentIds, CancellationToken cancellationToken = default)
        {
            var idList = parentIds.Distinct().ToList();
            if (idList.Count == 0)
                return [];

            return await _context.FinanceTransactions
                .AsNoTracking()
                .Where(t => t.CorrectsTransactionId != null && idList.Contains(t.CorrectsTransactionId.Value))
                .OrderBy(t => t.OccurredOn)
                .ThenBy(t => t.Id)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
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
            // Corrections never appear at the top level; they are embedded in their parent, so TotalCount
            // deliberately counts parents only and the FE's row count stays unchanged.
            var query = _context.FinanceTransactions
                .AsNoTracking()
                .Where(t => t.UserProfileId == userProfileId && t.CorrectsTransactionId == null);

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

        public async Task<IReadOnlyList<FinanceTransaction>> GetForRecurringTemplateAsync(
            int recurringTransactionId, CancellationToken cancellationToken = default)
        {
            return await _context.FinanceTransactions
                .Where(t => t.RecurringTransactionId == recurringTransactionId)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<MonthlyTotal>> GetMonthlyTotalsAsync(
            int userProfileId, int upToYear, int upToMonth, CancellationToken cancellationToken = default)
        {
            // Amount - CorrectedAmount rather than NetAmount: both are plain columns, so the SUM translates to
            // SQL. NetAmount is a computed CLR property and would force the whole history into memory.
            var cutoff = new DateOnly(upToYear, upToMonth, 1).AddMonths(1);

            var totals = await _context.FinanceTransactions
                .AsNoTracking()
                .Where(t => t.UserProfileId == userProfileId
                    && t.CorrectsTransactionId == null
                    && t.OccurredOn < cutoff)
                .GroupBy(t => new { t.OccurredOn.Year, t.OccurredOn.Month, t.Type })
                .Select(g => new MonthlyTotal(
                    g.Key.Year,
                    g.Key.Month,
                    g.Key.Type,
                    g.Sum(t => t.Amount - t.CorrectedAmount)))
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return totals
                .OrderBy(t => t.Year)
                .ThenBy(t => t.Month)
                .ToList();
        }

        public async Task<IReadOnlyList<FinanceTransaction>> GetForPeriodAsync(
            int userProfileId,
            DateOnly from,
            DateOnly to,
            FinanceTransactionTypeEnum? type,
            CancellationToken cancellationToken = default)
        {
            // Excluding corrections here is what keeps them out of all five analytics queries: their value is
            // already netted into the parent's NetAmount, in the parent's period.
            var query = _context.FinanceTransactions
                .AsNoTracking()
                .Where(t => t.UserProfileId == userProfileId
                    && t.CorrectsTransactionId == null
                    && t.OccurredOn >= from && t.OccurredOn <= to);

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
