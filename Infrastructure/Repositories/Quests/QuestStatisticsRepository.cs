using Domain.Interfaces.Authentication;
using Domain.Models;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories.Quests
{
    public class QuestStatisticsRepository : IQuestStatisticsRepository
    {
        private readonly AppDbContext _context;

        public QuestStatisticsRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task UpdateQuestStatisticsAsync(QuestStatistics stats, CancellationToken cancellationToken = default)
        {
            _context.QuestStatistics.Update(stats);
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
