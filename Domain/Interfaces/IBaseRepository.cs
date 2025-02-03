namespace Domain.Interfaces
{
    namespace Domain.Interfaces
    {
        public interface IBaseRepository<T> where T : class
        {
            Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
            Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
            Task AddAsync(T entity, CancellationToken cancellationToken = default);
            Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
            Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default);
        }
    }
}
