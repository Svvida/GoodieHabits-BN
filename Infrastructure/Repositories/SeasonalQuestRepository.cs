using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SeasonalQuestRepository : BaseRepository<SeasonalQuest>, ISeasonalQuestRepository
    {
        public SeasonalQuestRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<SeasonalQuest>> GetActiveQuestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<SeasonalQuest>()
                .Where(quest => quest.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<SeasonalQuest>> GetImportantQuestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<SeasonalQuest>()
                .Where(quest => quest.IsImportant)
                .ToListAsync(cancellationToken);
        }
    }
}
