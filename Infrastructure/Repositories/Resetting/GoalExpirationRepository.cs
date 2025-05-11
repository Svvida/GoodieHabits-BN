using Domain.Interfaces.Resetting;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Resetting
{
    public class GoalExpirationRepository : IGoalExpirationRepository
    {
        private readonly AppDbContext _context;

        public GoalExpirationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> ExpireGoalsAsync(CancellationToken cancellationToken = default)
        {
            var updatedGoals = await _context.UserGoals
                .Where(ug => !ug.IsExpired && ug.EndsAt <= DateTime.UtcNow)
                .ExecuteUpdateAsync(setter => setter
                    .SetProperty(p => p.IsExpired, true), cancellationToken)
                .ConfigureAwait(false);

            return updatedGoals;
        }
    }
}
