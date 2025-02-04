using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class WeeklyQuestRepository : BaseRepository<WeeklyQuest>, IWeeklyQuestRepository
    {
        public WeeklyQuestRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _context.QuestsMetadata.FindAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"WeeklyQuest with id {id} not found");

            _context.QuestsMetadata.Remove(quest);
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
