using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IFinanceTransactionRepository : IBaseRepository<FinanceTransaction>
    {
        Task<FinanceTransaction?> GetOwnedByIdAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>
        /// Same as <see cref="GetOwnedByIdAsync"/> but with the transaction's corrections loaded. A separate
        /// method so the existing call sites keep their cheaper query.
        /// </summary>
        Task<FinanceTransaction?> GetOwnedWithCorrectionsAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>Corrections belonging to the given parent transactions, for embedding into their DTOs.</summary>
        Task<IReadOnlyList<FinanceTransaction>> GetCorrectionsForParentsAsync(
            IEnumerable<int> parentIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Filtered, paged list of the user's transactions plus the total matching count. Corrections are
        /// excluded and returned embedded in their parent instead, so a correction dated in another month can
        /// never be paged away from the transaction it belongs to.
        /// </summary>
        Task<(IReadOnlyList<FinanceTransaction> Items, int TotalCount)> GetUserTransactionsAsync(
            int userProfileId,
            DateOnly? from,
            DateOnly? to,
            FinanceTransactionTypeEnum? type,
            int? categoryId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// All of the user's transactions in the inclusive date range, optionally filtered by type. For analytics.
        /// Corrections are excluded — they are already netted into their parent's <c>NetAmount</c>, which is what
        /// analytics aggregates, and they belong to the parent's period regardless of their own date.
        /// </summary>
        Task<IReadOnlyList<FinanceTransaction>> GetForPeriodAsync(
            int userProfileId,
            DateOnly from,
            DateOnly to,
            FinanceTransactionTypeEnum? type,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Netted per-month, per-type totals for everything the user logged up to and including
        /// (<paramref name="upToYear"/>, <paramref name="upToMonth"/>), oldest first. A grouped projection, so
        /// the opening-balance fold never loads transaction rows. Corrections are excluded and their value is
        /// taken off the parent's month, consistent with every other aggregate.
        /// </summary>
        Task<IReadOnlyList<MonthlyTotal>> GetMonthlyTotalsAsync(
            int userProfileId, int upToYear, int upToMonth, CancellationToken cancellationToken = default);

        /// <summary>
        /// Tracked transactions already materialized from a recurring template. Used to detach them before the
        /// template is deleted, and to skip months that have already been generated.
        /// </summary>
        Task<IReadOnlyList<FinanceTransaction>> GetForRecurringTemplateAsync(
            int recurringTransactionId, CancellationToken cancellationToken = default);

        /// <summary>True if any transaction references any of the given category ids. Used as a delete guard.</summary>
        Task<bool> AnyForCategoriesAsync(IEnumerable<int> categoryIds, CancellationToken cancellationToken = default);
    }
}
