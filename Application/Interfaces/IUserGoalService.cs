using Application.Dtos.UserGoal;

namespace Application.Interfaces
{
    public interface IUserGoalService
    {
        Task CreateUserGoalAsync(CreateUserGoalDto createUserGoalDto, CancellationToken cancellationToken = default);
    }
}
