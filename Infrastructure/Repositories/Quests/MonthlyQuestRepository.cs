using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Quests
{
    public class MonthlyQuestRepository : BaseRepository<MonthlyQuest>, IMonthlyQuestRepository
    {
        public MonthlyQuestRepository(AppDbContext context) : base(context) { }
    }
}
