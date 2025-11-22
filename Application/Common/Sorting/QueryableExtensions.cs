using System.Linq.Expressions;

namespace Application.Common.Sorting
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyOrder<T, TKey>(
            this IQueryable<T> query,
            Expression<Func<T, TKey>> keySelector,
            SortOrder order)
        {
            return order == SortOrder.Asc
                ? query.OrderBy(keySelector)
                : query.OrderByDescending(keySelector);
        }
    }
}
