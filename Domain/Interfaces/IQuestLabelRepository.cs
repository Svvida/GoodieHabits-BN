using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces
{
    public interface IQuestLabelRepository : IBaseRepository<QuestLabel>
    {
        Task<IEnumerable<QuestLabel>> GetUserLabelsAsync(int accountId, bool asNoTracking, CancellationToken cancellationToken = default);
        Task<QuestLabel?> GetLabelByValueAsync(string value, int accountId, CancellationToken cancellationToken = default);
        Task<bool> IsLabelOwnedByUserAsync(int labelId, int accountId, CancellationToken cancellationToken = default);
        Task<int> CountOwnedLabelsAsync(IEnumerable<int> labelIds, int accountId, CancellationToken cancellationToken = default);
    }
}
