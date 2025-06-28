namespace Application.Interfaces
{
    public interface IGoalExpirationService
    {
        Task<int> ExpireGoalsAndSaveAsync(CancellationToken cancellationToken = default);
    }
}
