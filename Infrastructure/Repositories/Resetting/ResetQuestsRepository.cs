using Domain.Interfaces.Resetting;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Resetting
{
    public class ResetQuestsRepository : IResetQuestsRepository
    {
        private readonly AppDbContext _context;

        public ResetQuestsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> ResetQuestsAsync(CancellationToken cancellationToken = default)
        {
            var resetedQuests = await _context.Quests
                .Where(q => q.IsCompleted == true && q.NextResetAt <= DateTime.UtcNow)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.IsCompleted, false)
                    .SetProperty(p => p.NextResetAt, (DateTime?)null), cancellationToken)
                .ConfigureAwait(false);
            return resetedQuests;
        }
    }
}

