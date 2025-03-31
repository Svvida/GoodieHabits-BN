using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Quests
{
    public class ResetQuestsRepository : IResetQuestsRepository
    {
        private readonly AppDbContext _context;

        public ResetQuestsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task ResetQuestsAsync(CancellationToken cancellationToken = default)
        {
            await _context.Quests
                .Where(q => q.IsCompleted == true && q.NextResetAt <= DateTime.UtcNow)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.IsCompleted, false)
                    .SetProperty(p => p.NextResetAt, (DateTime?)null))
                .ConfigureAwait(false);
        }
    }
}

