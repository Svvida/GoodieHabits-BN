namespace Domain.Interfaces
{
    public interface IGoalExpirationRepository
    {
        Task<int> ExpireGoalsAsync(CancellationToken cancellationToken = default);
    }
}
