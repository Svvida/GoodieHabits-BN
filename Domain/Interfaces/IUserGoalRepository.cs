using Domain.Enum;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IUserGoalRepository
    {
        Task CreateAsync(UserGoal userGoal, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserGoal userGoal, CancellationToken cancellationToken = default);
        Task<int> GetActiveGoalsCountByTypeAsync(int accountId, GoalTypeEnum goalType, CancellationToken cancellationToken = default);
        Task<UserGoal?> GetActiveGoalByQuestIdAsync(int questId, CancellationToken cancellationToken = default);
        Task<UserGoal?> GetUserActiveGoalByTypeAsync(int accountId, GoalTypeEnum goalType, CancellationToken cancellationToken = default);
        Task<bool> IsQuestActiveGoalAsync(int questId, CancellationToken cancellationToken = default);
    }
}
