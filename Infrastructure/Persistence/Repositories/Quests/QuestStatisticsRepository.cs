using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Quests
{
    public class QuestStatisticsRepository(AppDbContext context) : BaseRepository<QuestStatistics>(context), IQuestStatisticsRepository
    {
        public async Task<QuestStatistics?> GetStatisticsForQuestAsync(int questId,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
        {
            var query = _context.QuestStatistics.Where(q => q.QuestId == questId);

            if (asNoTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
