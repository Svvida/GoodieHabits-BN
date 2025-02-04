using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories
{
    public class SeasonalQuestRepository : BaseRepository<SeasonalQuest>, ISeasonalQuestRepository
    {
        public SeasonalQuestRepository(AppDbContext context) : base(context)
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
