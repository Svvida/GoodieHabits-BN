namespace Domain.Interfaces
{
    namespace Domain.Interfaces
    {
        public interface IBaseRepository<T> where T : class
        {
            Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
            Task AddAsync(T entity, CancellationToken cancellationToken = default);
            Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
            void Delete(T entity);
        }
    }
}
