namespace Domain.Interfaces
{
    public interface IResetQuestsRepository
    {
        Task ResetDailyQuestsAsync(CancellationToken cancellationToken = default);
    }
}
