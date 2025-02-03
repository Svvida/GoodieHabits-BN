using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DailyQuestRepository : BaseRepository<DailyQuest>, IDailyQuestRepository
    {
        public DailyQuestRepository(AppDbContext context) : base(context) { }

        public override async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _context.QuestsMetadata.FirstOrDefaultAsync(q => q.Id == id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {id} not found.");

            _context.QuestsMetadata.Remove(quest);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}