namespace Domain.Interfaces.Resetting
{
    public interface IResetQuestsRepository
    {
        Task<int> ResetQuestsAsync(CancellationToken cancellationToken = default);
    }
}
