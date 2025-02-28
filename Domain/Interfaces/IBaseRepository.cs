using System.Linq.Expressions;

namespace Domain.Interfaces
{
    namespace Domain.Interfaces
    {
        public interface IBaseRepository<T> where T : class
        {
            Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
            Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
            Task<IEnumerable<T>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
            Task AddAsync(T entity, CancellationToken cancellationToken = default);
            Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
            Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
        }
    }
}
