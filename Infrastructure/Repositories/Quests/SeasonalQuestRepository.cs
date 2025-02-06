using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Quests
{
    public class SeasonalQuestRepository : QuestBaseRepository<SeasonalQuest>, ISeasonalQuestRepository
    {
        public SeasonalQuestRepository(AppDbContext context) : base(context) { }
    }
}
