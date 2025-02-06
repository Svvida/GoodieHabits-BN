using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Quests
{
    public class MonthlyQuestRepository : QuestBaseRepository<MonthlyQuest>, IMonthlyQuestRepository
    {
        public MonthlyQuestRepository(AppDbContext context) : base(context) { }
    }
}
