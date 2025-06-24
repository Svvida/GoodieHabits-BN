namespace Domain.Interfaces.Resetting
{
    public interface IResetQuestsRepository
    {
        Task PrepareQuestsForResetAsync(CancellationToken cancellationToken = default);
    }
}
