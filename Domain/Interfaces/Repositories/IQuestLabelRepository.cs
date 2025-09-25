using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Repositories
{
    public interface IQuestLabelRepository : IBaseRepository<QuestLabel>
    {
        Task<QuestLabel?> GetUserLabelByIdAsync(int labelId, int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);
        Task<IEnumerable<QuestLabel>> GetUserLabelsAsync(int userProfileId, bool asNoTracking, CancellationToken cancellationToken = default);
        Task<bool> IsLabelValueUniqueForUser(string value, int userProfileId, CancellationToken cancellationToken = default);
        Task<bool> IsLabelOwnedByUserAsync(int labelId, int userProfileId, CancellationToken cancellationToken = default);
        Task<int> CountOwnedLabelsAsync(IEnumerable<int> labelIds, int userProfileId, CancellationToken cancellationToken = default);
    }
}
