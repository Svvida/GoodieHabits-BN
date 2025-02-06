using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Quests
{
    public class QuestMetadataRepository : IQuestMetadataRepository
    {
        private readonly AppDbContext _context;

        public QuestMetadataRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<QuestMetadata>> GetAllQuestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.QuestsMetadata
                .Include(q => q.OneTimeQuest)
                .Include(q => q.DailyQuest)
                .Include(q => q.WeeklyQuest)
                .Include(q => q.MonthlyQuest)
                .Include(q => q.SeasonalQuest)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
