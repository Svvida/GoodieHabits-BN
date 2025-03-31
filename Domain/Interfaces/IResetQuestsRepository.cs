namespace Domain.Interfaces
{
    public interface IResetQuestsRepository
    {
        Task ResetQuestsAsync(CancellationToken cancellationToken = default);
    }
}
