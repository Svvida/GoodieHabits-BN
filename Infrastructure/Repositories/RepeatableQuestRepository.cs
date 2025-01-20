using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RepeatableQuestRepository : BaseRepository<RepeatableQuest>, IRepeatableQuestRepository
    {
        public RepeatableQuestRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<RepeatableQuest>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RepeatableQuest>()
                .Where(quest => quest.AccountId == accountId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RepeatableQuest>> GetDueQuestsAsync(TimeOnly currentTime, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RepeatableQuest>()
                .Where(quest => quest.RepeatTime == currentTime)
                .ToListAsync(cancellationToken);
        }
    }
}
