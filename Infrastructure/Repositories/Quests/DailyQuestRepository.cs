using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Quests
{
    public class DailyQuestRepository : QuestBaseRepository<DailyQuest>, IDailyQuestRepository
    {
        public DailyQuestRepository(AppDbContext context) : base(context) { }
    }
}