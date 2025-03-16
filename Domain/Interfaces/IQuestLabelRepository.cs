using Domain.Models;

namespace Domain.Interfaces
{
    public interface IQuestLabelRepository
    {
        Task<QuestLabel?> GetLabelByIdAsync(int labelId, int accountId, CancellationToken cancellationToken = default);
        Task<IEnumerable<QuestLabel>> GetUserLabelsAsync(int accountId, CancellationToken cancellationToken = default);
        Task CreateLabelAsync(QuestLabel label, CancellationToken cancellationToken = default);
        Task UpdateLabelAsync(QuestLabel label, CancellationToken cancellationToken = default);
        Task DeleteLabelAsync(QuestLabel label, CancellationToken cancellationToken = default);
        Task<QuestLabel?> GetLabelByValueAsync(string value, int accountId, CancellationToken cancellationToken = default);
    }
}
