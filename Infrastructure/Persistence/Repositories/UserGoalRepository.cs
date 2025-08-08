using Domain.Enum;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserGoalRepository : BaseRepository<UserGoal>, IUserGoalRepository
    {

        public UserGoalRepository(AppDbContext context) : base(context) { }

        public async Task<int> GetActiveGoalsCountByTypeAsync(int accountId, GoalTypeEnum goalType, CancellationToken cancellationToken = default)
        {
            return await _context.UserGoals.CountAsync
                (ug =>
                ug.AccountId == accountId &&
                ug.GoalType == goalType &&
                !ug.IsExpired, cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserGoal?> GetActiveGoalByQuestIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            return await _context.UserGoals
                .FirstOrDefaultAsync(ug => ug.QuestId == questId && !ug.IsExpired, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<UserGoal?> GetUserActiveGoalByTypeAsync(int accountId, GoalTypeEnum goalType, CancellationToken cancellationToken = default)
        {
            return await _context.UserGoals
                .Include(ug => ug.Quest)
                .FirstOrDefaultAsync(ug => ug.AccountId == accountId && ug.GoalType == goalType && !ug.IsExpired, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<bool> IsQuestActiveGoalAsync(int questId, CancellationToken cancellationToken = default)
        {
            return await _context.UserGoals
                .AnyAsync(ug => ug.QuestId == questId && !ug.IsExpired, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<UserGoal>> GetGoalsToExpireAsync(CancellationToken cancellationToken = default)
        {
            return await _context.UserGoals
                .Where(ug => !ug.IsExpired && ug.EndsAt <= DateTime.UtcNow)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }
}