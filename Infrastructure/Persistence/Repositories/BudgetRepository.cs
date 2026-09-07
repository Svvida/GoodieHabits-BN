using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class BudgetRepository(AppDbContext context) : BaseRepository<Budget>(context), IBudgetRepository
    {
        public async Task<Budget?> GetOwnedByIdAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.Budgets.Where(b => b.Id == id && b.UserProfileId == userProfileId);
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<Budget>> GetUserBudgetsAsync(int userProfileId, int year, int? month, bool asNoTracking, CancellationToken cancellationToken = default)
        {
            var query = _context.Budgets.Where(b => b.UserProfileId == userProfileId && b.Year == year);

            // When a month is requested, return that month's monthly budgets plus the year's overall (yearly) budgets.
            if (month.HasValue)
                query = query.Where(b => b.Month == month || b.Month == null);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query
                .OrderBy(b => b.CategoryId == null ? 0 : 1)
                .ThenBy(b => b.Period)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsForPeriodAsync(
            int userProfileId,
            int? categoryId,
            BudgetPeriodEnum period,
            int year,
            int? month,
            int? excludeBudgetId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Budgets
                .AnyAsync(
                    b => b.UserProfileId == userProfileId
                        && b.CategoryId == categoryId
                        && b.Period == period
                        && b.Year == year
                        && b.Month == month
                        && (excludeBudgetId == null || b.Id != excludeBudgetId),
                    cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> AnyForCategoriesAsync(IEnumerable<int> categoryIds, CancellationToken cancellationToken = default)
        {
            var idList = categoryIds.Distinct().ToList();
            if (idList.Count == 0)
                return false;

            return await _context.Budgets
                .AnyAsync(b => b.CategoryId != null && idList.Contains(b.CategoryId.Value), cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
