using Domain.Enums;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Infrastructure.Persistence.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UserGoalRepository(AppDbContext context) : BaseRepository<UserGoal>(context), IUserGoalRepository
    {
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
    }
}