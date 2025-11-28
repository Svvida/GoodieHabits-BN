using Domain.Enums;
using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IUserGoalRepository : IBaseRepository<UserGoal>
    {
        Task<int> GetActiveGoalsCountByTypeAsync(int userProfileId, GoalTypeEnum goalType, CancellationToken cancellationToken = default);
        Task<UserGoal?> GetActiveGoalByQuestIdAsync(int questId, CancellationToken cancellationToken = default);
        Task<UserGoal?> GetUserActiveGoalByTypeAsync(int userProfileId, GoalTypeEnum goalType, CancellationToken cancellationToken = default);
        Task<bool> IsQuestActiveGoalAsync(int questId, CancellationToken cancellationToken = default);
    }
}
