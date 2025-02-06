using Domain.Common;
using Domain.Exceptions;
using Domain.Interfaces.Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Common
{
    public abstract class QuestBaseRepository<T> : BaseRepository<T>, IBaseRepository<T> where T : QuestBase
    {
        public QuestBaseRepository(AppDbContext context) : base(context) { }

        public override async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _context.QuestsMetadata.FirstOrDefaultAsync(q => q.Id == id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {id} not found.");

            _context.QuestsMetadata.Remove(quest);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
