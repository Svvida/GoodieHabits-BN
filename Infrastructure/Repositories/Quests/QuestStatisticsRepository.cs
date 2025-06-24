using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Quests
{
    public class QuestStatisticsRepository : BaseRepository<QuestStatistics>, IQuestStatisticsRepository
    {

        public QuestStatisticsRepository(AppDbContext context) : base(context) { }
    }
}
