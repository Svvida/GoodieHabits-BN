using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class RecurringQuestRepository : BaseRepository<RecurringQuest>, IRecurringQuestRepository
    {
        public RecurringQuestRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<RecurringQuest>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RecurringQuest>()
                .Where(quest => quest.AccountId == accountId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RecurringQuest>> GetDueQuestsAsync(TimeOnly currentTime, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RecurringQuest>()
                .Where(quest => quest.RepeatTime == currentTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RecurringQuest>> GetImportantQuestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<RecurringQuest>()
                .Where(quest => quest.IsImportant)
                .ToListAsync(cancellationToken);
        }
    }
}
