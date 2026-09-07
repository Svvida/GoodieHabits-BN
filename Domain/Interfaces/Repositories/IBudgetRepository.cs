using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IBudgetRepository : IBaseRepository<Budget>
    {
        Task<Budget?> GetOwnedByIdAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>The user's budgets for a given year, optionally narrowed to a specific month.</summary>
        Task<IReadOnlyList<Budget>> GetUserBudgetsAsync(int userProfileId, int year, int? month, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>True if a budget already exists for the same scope/period (optionally excluding one id).</summary>
        Task<bool> ExistsForPeriodAsync(
            int userProfileId,
            int? categoryId,
            BudgetPeriodEnum period,
            int year,
            int? month,
            int? excludeBudgetId,
            CancellationToken cancellationToken = default);

        /// <summary>True if any budget is scoped to one of these categories. Guards category deletion.</summary>
        Task<bool> AnyForCategoriesAsync(IEnumerable<int> categoryIds, CancellationToken cancellationToken = default);
    }
}
