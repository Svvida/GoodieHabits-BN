using Application.Common.Dtos;
using Application.Finance.Transactions.Dtos;
using Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace Application.Finance.Transactions.Queries.GetTransactions
{
    public class GetTransactionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        : IRequestHandler<GetTransactionsQuery, PagedResult<TransactionDto>>
    {
        public async Task<PagedResult<TransactionDto>> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
        {
            var categoryIds = await ResolveCategoryScopeAsync(request.CategoryIds, cancellationToken).ConfigureAwait(false);

            var (items, totalCount) = await unitOfWork.FinanceTransactions
                .GetUserTransactionsAsync(
                    request.UserProfileId,
                    request.From,
                    request.To,
                    request.Type,
                    categoryIds,
                    request.IsPaid,
                    request.Search,
                    request.Page,
                    request.PageSize,
                    cancellationToken)
                .ConfigureAwait(false);

            var dtos = items.Select(mapper.Map<TransactionDto>).ToList();

            // Corrections are excluded from the page itself and attached to their parent here — a correction may
            // fall outside the requested filter (a different month, typically) and must still travel with it.
            var corrections = await unitOfWork.FinanceTransactions
                .GetCorrectionsForParentsAsync(items.Select(t => t.Id), cancellationToken).ConfigureAwait(false);

            if (corrections.Count > 0)
            {
                var byParent = corrections
                    .GroupBy(c => c.CorrectsTransactionId!.Value)
                    .ToDictionary(group => group.Key, group => group.Select(mapper.Map<TransactionDto>).ToList());

                foreach (var dto in dtos)
                {
                    if (byParent.TryGetValue(dto.Id, out var parentCorrections))
                        dto.Corrections = parentCorrections;
                }
            }

            return new PagedResult<TransactionDto>(dtos, request.Page, request.PageSize, totalCount);
        }

        /// <summary>
        /// Filtering by a main category means "everything filed under it", sub-categories included — the same
        /// reading <c>GetBudgetProgressQueryHandler</c> gives a budget, so "Food" cannot mean one thing on the
        /// dashboard and another in history. Narrowing to a single sub is done by asking for that sub's id: a
        /// sub has no children, so this expansion leaves it untouched. The tree is two levels deep, so one
        /// lookup is the whole story. No ownership check is needed — the query is scoped to the caller's own
        /// transactions regardless of which ids land in the set.
        /// </summary>
        private async Task<IReadOnlyCollection<int>?> ResolveCategoryScopeAsync(
            IReadOnlyList<int>? requestedIds, CancellationToken cancellationToken)
        {
            if (requestedIds is not { Count: > 0 })
                return null;

            var scope = requestedIds.ToHashSet();

            var subCategoryIds = await unitOfWork.FinanceCategories
                .GetSubCategoryIdsAsync(scope, cancellationToken).ConfigureAwait(false);

            scope.UnionWith(subCategoryIds);

            return scope;
        }
    }
}
