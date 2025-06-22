using Domain.Enum;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IUserGoalRepository : IBaseRepository<UserGoal>
    {
        Task<int> GetActiveGoalsCountByTypeAsync(int accountId, GoalTypeEnum goalType, CancellationToken cancellationToken = default);
        Task<UserGoal?> GetActiveGoalByQuestIdAsync(int questId, CancellationToken cancellationToken = default);
        Task<UserGoal?> GetUserActiveGoalByTypeAsync(int accountId, GoalTypeEnum goalType, CancellationToken cancellationToken = default);
        Task<bool> IsQuestActiveGoalAsync(int questId, CancellationToken cancellationToken = default);
        Task<IEnumerable<UserGoal>> GetGoalsToExpireAsync(CancellationToken cancellationToken = default);
    }
}
