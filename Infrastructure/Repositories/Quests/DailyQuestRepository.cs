using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Quests
{
    public class DailyQuestRepository : BaseRepository<DailyQuest>, IDailyQuestRepository
    {
        public DailyQuestRepository(AppDbContext context) : base(context) { }
    }
}