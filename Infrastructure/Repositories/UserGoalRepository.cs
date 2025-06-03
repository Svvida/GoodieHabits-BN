using Domain.Enum;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class UserGoalRepository : IUserGoalRepository
    {
        private readonly AppDbContext _context;

        public UserGoalRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(UserGoal userGoal, CancellationToken cancellationToken = default)
        {
            await _context.UserGoals.AddAsync(userGoal, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(UserGoal userGoal, CancellationToken cancellationToken = default)
        {
            _context.UserGoals.Update(userGoal);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<int> GetActiveGoalsCountByTypeAsync(int accountId, GoalTypeEnum goalType, CancellationToken cancellationToken = default)
        {
            return await _context.UserGoals.CountAsync
                (ug =>
                ug.AccountId == accountId &&
                ug.GoalType == goalType &&
                !ug.IsAchieved &&
                !ug.IsExpired, cancellationToken).ConfigureAwait(false);
        }

        public async Task<UserGoal?> GetActiveGoalByQuestIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            return await _context.UserGoals
                .FirstOrDefaultAsync(ug => ug.QuestId == questId && !ug.IsAchieved && !ug.IsExpired, cancellationToken)
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