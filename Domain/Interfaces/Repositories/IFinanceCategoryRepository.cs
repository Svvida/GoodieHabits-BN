using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IFinanceCategoryRepository : IBaseRepository<FinanceCategory>
    {
        /// <summary>System categories plus the user's own, mains with their sub-categories loaded.</summary>
        Task<IEnumerable<FinanceCategory>> GetUserCategoryTreeAsync(int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>A category owned by the user (excludes system categories). Used for edit/delete.</summary>
        Task<FinanceCategory?> GetOwnedByIdAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>A category the user may reference (own or system). Used for parent selection and transaction assignment.</summary>
        Task<FinanceCategory?> GetAssignableByIdAsync(int id, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);

        /// <summary>The user's own categories matching the given ids. Used for bulk delete.</summary>
        Task<IReadOnlyList<FinanceCategory>> GetOwnedByIdsAsync(IEnumerable<int> ids, int userProfileId, CancellationToken cancellationToken = default);

        /// <summary>Ids of sub-categories belonging to any of the given parent ids.</summary>
        Task<IReadOnlyList<int>> GetSubCategoryIdsAsync(IEnumerable<int> parentIds, CancellationToken cancellationToken = default);

        /// <summary>The user's own sub-categories under the given parent, tracked. Used to cascade parent-inherited changes.</summary>
        Task<IReadOnlyList<FinanceCategory>> GetOwnedSubCategoriesAsync(int parentCategoryId, int userProfileId, CancellationToken cancellationToken = default);

        Task<bool> IsNameUniqueUnderParentAsync(
            string name,
            int userProfileId,
            FinanceTransactionTypeEnum type,
            int? parentCategoryId,
            int? excludeCategoryId,
            CancellationToken cancellationToken = default);
    }
}
