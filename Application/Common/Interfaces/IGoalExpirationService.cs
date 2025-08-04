namespace Application.Common.Interfaces
{
    public interface IGoalExpirationService
    {
        Task<int> ExpireGoalsAndSaveAsync(CancellationToken cancellationToken = default);
    }
}
