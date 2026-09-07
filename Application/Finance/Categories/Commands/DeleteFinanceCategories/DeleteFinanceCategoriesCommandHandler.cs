using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;

namespace Application.Finance.Categories.Commands.DeleteFinanceCategories
{
    /// <summary>
    /// Bulk-deletes the user's own categories. All-or-nothing: the request is rejected (and nothing is
    /// deleted) if any id is not owned, is still referenced by a transaction, a recurring template or a
    /// budget, or is a main whose sub-categories are not all included in the same request.
    /// </summary>
    public class DeleteFinanceCategoriesCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteFinanceCategoriesCommand, Unit>
    {
        public async Task<Unit> Handle(DeleteFinanceCategoriesCommand request, CancellationToken cancellationToken)
        {
            var ids = request.CategoryIds.Distinct().ToList();

            var owned = await unitOfWork.FinanceCategories
                .GetOwnedByIdsAsync(ids, request.UserProfileId, cancellationToken).ConfigureAwait(false);

            if (owned.Count != ids.Count)
            {
                var foundIds = owned.Select(c => c.Id).ToHashSet();
                var missing = ids.Where(id => !foundIds.Contains(id));
                throw new NotFoundException($"Categories not found or not owned by you: {string.Join(", ", missing)}.");
            }

            var hasTransactions = await unitOfWork.FinanceTransactions
                .AnyForCategoriesAsync(ids, cancellationToken).ConfigureAwait(false);

            if (hasTransactions)
                throw new ConflictException("One or more selected categories have transactions and cannot be deleted.");

            // Templates and budgets hold the same Restrict FK as transactions do. Without these checks the
            // delete reaches the database and surfaces as a DbUpdateException (500) instead of a 409 — and a
            // template referencing a category it has not materialized anything for yet is easy to hit.
            var hasRecurringTemplates = await unitOfWork.RecurringTransactions
                .AnyForCategoriesAsync(ids, cancellationToken).ConfigureAwait(false);

            if (hasRecurringTemplates)
                throw new ConflictException(
                    "One or more selected categories are used by a recurring transaction and cannot be deleted.");

            var hasBudgets = await unitOfWork.Budgets
                .AnyForCategoriesAsync(ids, cancellationToken).ConfigureAwait(false);

            if (hasBudgets)
                throw new ConflictException("One or more selected categories have budgets and cannot be deleted.");

            var mainIds = owned.Where(c => c.IsMain).Select(c => c.Id).ToList();
            if (mainIds.Count > 0)
            {
                var childIds = await unitOfWork.FinanceCategories
                    .GetSubCategoryIdsAsync(mainIds, cancellationToken).ConfigureAwait(false);

                var idSet = ids.ToHashSet();
                if (childIds.Any(childId => !idSet.Contains(childId)))
                    throw new ConflictException(
                        "A main category with sub-categories cannot be deleted unless all of its sub-categories are included in the request.");
            }

            unitOfWork.FinanceCategories.RemoveRange(owned);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }
    }
}
