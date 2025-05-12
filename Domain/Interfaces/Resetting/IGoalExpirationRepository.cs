namespace Domain.Interfaces.Resetting
{
    public interface IGoalExpirationRepository
    {
        Task<int> ExpireGoalsAsync(CancellationToken cancellationToken = default);
    }
}
