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
            // Get expired goals
            var expiredGoals = await _context.UserGoals
                .Where(ug => !ug.IsExpired && ug.EndsAt <= DateTime.UtcNow)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Expire goals
            foreach (var goal in expiredGoals)
            {
                goal.IsExpired = true;
            }

            // Aggregate expired goals by accountId
            var accountGoalCounts = new Dictionary<int, int>();

            foreach (var goal in expiredGoals)
            {
                if (accountGoalCounts.TryGetValue(goal.AccountId, out int value))
                    accountGoalCounts[goal.AccountId] = value + 1;
                else
                    accountGoalCounts[goal.AccountId] = 1;
            }

            // Fetch UserProfiles in Bulk
            var accountIds = accountGoalCounts.Keys.ToList();
            var userProfiles = await _context.UserProfiles
                .Where(up => accountIds.Contains(up.AccountId))
                .AsNoTracking()
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            // Update UserProfiles
            foreach (var profile in userProfiles)
            {
                if (accountGoalCounts.TryGetValue(profile.AccountId, out int value))
                    profile.ExpiredGoals += value;
            }

            // Save changes
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return expiredGoals.Count;
        }
    }
}
