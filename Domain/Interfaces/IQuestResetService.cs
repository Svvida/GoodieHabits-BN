using Domain.Models;

namespace Domain.Interfaces
{
    public interface IQuestResetService
    {
        DateTime? GetNextResetTimeUtc(Quest quest);
        Task<int> ResetCompletedQuestsAsync(CancellationToken cancellationToken = default);
    }
}
