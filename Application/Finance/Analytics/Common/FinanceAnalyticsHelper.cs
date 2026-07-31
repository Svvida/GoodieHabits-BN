using Application.Finance.Analytics.Dtos;
using Domain.Models;

namespace Application.Finance.Analytics.Common
{
    internal static class FinanceAnalyticsHelper
    {
        /// <summary>
        /// Groups transactions by their assigned category and computes each category's amount and its
        /// share of the group total. Category names/parents are resolved from the provided lookup.
        /// Sums <c>NetAmount</c>: what came back via corrections was never really spent or earned.
        /// </summary>
        public static List<CategoryBreakdownItemDto> BuildBreakdown(
            IReadOnlyCollection<FinanceTransaction> transactions,
            IReadOnlyDictionary<int, FinanceCategory> categoriesById)
        {
            var total = transactions.Sum(t => t.NetAmount);

            return transactions
                .GroupBy(t => t.CategoryId)
                .Select(group =>
                {
                    FinanceCategory? category =
                        group.Key is int id && categoriesById.TryGetValue(id, out var found) ? found : null;

                    var amount = group.Sum(t => t.NetAmount);

                    return new CategoryBreakdownItemDto
                    {
                        CategoryId = group.Key,
                        CategoryName = category?.Name,
                        ParentCategoryId = category?.ParentCategoryId,
                        Amount = amount,
                        Percentage = total > 0
                            ? Math.Round(amount / total * 100m, 2, MidpointRounding.AwayFromZero)
                            : 0m,
                    };
                })
                .OrderByDescending(item => item.Amount)
                .ToList();
        }
    }
}
