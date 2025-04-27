namespace Domain.Interfaces
{
    public interface IResetQuestsRepository
    {
        Task<int> ResetQuestsAsync(CancellationToken cancellationToken = default);
    }
}
