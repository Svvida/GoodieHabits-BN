using Application.Dtos.Quests;
using Application.Dtos.UserGoal;
using Domain.Enum;

namespace Application.Interfaces
{
    public interface IUserGoalService
    {
        Task CreateUserGoalAsync(CreateUserGoalDto createUserGoalDto, CancellationToken cancellationToken = default);
        Task<BaseGetQuestDto?> GetUserActiveGoalByTypeAsync(int accountId, GoalTypeEnum goalType, CancellationToken cancellationToken = default);
    }
}
