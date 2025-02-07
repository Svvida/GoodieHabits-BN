using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Quests
{
    public class WeeklyQuestRepository : BaseRepository<WeeklyQuest>, IWeeklyQuestRepository
    {
        public WeeklyQuestRepository(AppDbContext context) : base(context) { }
    }
}
