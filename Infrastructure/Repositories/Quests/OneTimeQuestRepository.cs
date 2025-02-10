using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Infrastructure.Repositories.Common;

namespace Infrastructure.Repositories.Quests
{
    public class OneTimeQuestRepository : BaseRepository<OneTimeQuest>, IOneTimeQuestRepository
    {
        public OneTimeQuestRepository(AppDbContext context) : base(context) { }
    }
}
