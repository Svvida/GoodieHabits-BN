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

        public async Task ResetDailyQuestsAsync(CancellationToken cancellationToken = default)
        {
            await _context.DailyQuests
                .Where(q => q.IsCompleted == true &&
                            q.LastCompleted.HasValue &&
                            q.LastCompleted.Value.Date < DateTime.UtcNow.Date)
                        .ExecuteUpdateAsync(q => q.SetProperty(x => x.IsCompleted, false), cancellationToken)
                        .ConfigureAwait(false);
        }
    }
}
