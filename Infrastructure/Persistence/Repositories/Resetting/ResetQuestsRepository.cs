using Domain.Interfaces.Resetting;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Resetting
{
    public class ResetQuestsRepository : IResetQuestsRepository
    {
        private readonly AppDbContext _context;

        public ResetQuestsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task PrepareQuestsForResetAsync(CancellationToken cancellationToken = default)
        {
            var questsToReset = await _context.Quests
                .Where(q => q.IsCompleted == true && (q.EndDate ?? DateTime.MaxValue) >= DateTime.UtcNow && q.NextResetAt <= DateTime.UtcNow)
                .Include(q => q.Account)
                    .ThenInclude(a => a.Profile)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (questsToReset.Count == 0)
                return; // No quests to reset

            foreach (var quest in questsToReset)
            {
                quest.IsCompleted = false;
                quest.Account.Profile.CurrentlyCompletedExistingQuests = Math.Max(0, quest.Account.Profile.CurrentlyCompletedExistingQuests - 1);
            }
        }
    }
}

