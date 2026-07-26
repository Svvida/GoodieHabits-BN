using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IFinanceTransactionRepository : IBaseRepository<FinanceTransaction>
    {
        Task<FinanceTransaction?> GetOwnedByIdAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>Filtered, paged list of the user's transactions plus the total matching count.</summary>
        Task<(IReadOnlyList<FinanceTransaction> Items, int TotalCount)> GetUserTransactionsAsync(
            int userProfileId,
            DateOnly? from,
            DateOnly? to,
            FinanceTransactionTypeEnum? type,
            int? categoryId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>All of the user's transactions in the inclusive date range, optionally filtered by type. For analytics.</summary>
        Task<IReadOnlyList<FinanceTransaction>> GetForPeriodAsync(
            int userProfileId,
            DateOnly from,
            DateOnly to,
            FinanceTransactionTypeEnum? type,
            CancellationToken cancellationToken = default);

        /// <summary>True if any transaction references any of the given category ids. Used as a delete guard.</summary>
        Task<bool> AnyForCategoriesAsync(IEnumerable<int> categoryIds, CancellationToken cancellationToken = default);
    }
}
