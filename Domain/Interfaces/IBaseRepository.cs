using System.Linq.Expressions;

namespace Domain.Interfaces
{
    namespace Domain.Interfaces
    {
        public interface IBaseRepository<T> where T : class
        {
            Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
            Task AddAsync(T entity, CancellationToken cancellationToken = default);
            Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
            Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
            Task<bool> ExistsByFieldAsync<TValue>(Expression<Func<T, TValue>> field, TValue value, CancellationToken cancellationToken = default);
        }
    }
}
