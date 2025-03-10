using Domain.Interfaces.Quests;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Quests
{
    public class SeasonalQuestRepository : BaseRepository<SeasonalQuest>, ISeasonalQuestRepository
    {
        public SeasonalQuestRepository(AppDbContext context) : base(context) { }
    }
}
