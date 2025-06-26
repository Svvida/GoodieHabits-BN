using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Quests
{
    public class QuestStatisticsRepository : BaseRepository<QuestStatistics>, IQuestStatisticsRepository
    {

        public QuestStatisticsRepository(AppDbContext context) : base(context) { }

        public async Task<QuestStatistics?> GetStatisticsForQuestAsync(int questId,
            bool asNoTracking = false,
            CancellationToken cancellationToken = default)
        {
            var query = _context.QuestStatistics.Where(q => q.QuestId == questId);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
