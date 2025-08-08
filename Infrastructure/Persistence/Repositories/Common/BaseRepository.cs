using Domain.Interfaces.Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Common
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly AppDbContext _context;

        public BaseRepository(AppDbContext context)
        {
            _context = context;
        }
        public virtual async Task<T?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Id") == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().AnyAsync(entity => EF.Property<int>(entity, "Id") == id, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task ExecuteDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().Where(entity => EF.Property<int>(entity, "Id") == id)
                .ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        }
        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().AddAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);
        }

        public void Remove(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }
    }
}