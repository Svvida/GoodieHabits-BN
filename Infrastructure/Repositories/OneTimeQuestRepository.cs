using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class OneTimeQuestRepository : BaseRepository<OneTimeQuest>, IOneTimeQuestRepository
    {
        public OneTimeQuestRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<OneTimeQuest>> GetByAccountIdAsync(int accountId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<OneTimeQuest>()
                .Where(quest => quest.AccountId == accountId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OneTimeQuest>> GetOverdueQuestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<OneTimeQuest>()
                .Where(quest => quest.EndDate.HasValue && quest.EndDate < DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<OneTimeQuest>> GetImportantQuestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<OneTimeQuest>()
                .Where(quest => quest.IsImportant)
                .ToListAsync(cancellationToken);
        }

    }
}
