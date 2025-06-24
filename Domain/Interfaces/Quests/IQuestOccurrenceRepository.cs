using Domain.Interfaces.Domain.Interfaces;
using Domain.Models;

namespace Domain.Interfaces.Quests
{
    public interface IQuestOccurrenceRepository : IBaseRepository<QuestOccurrence>
    {
        Task<bool> IsQuestOccurrenceExistsAsync(int questId, DateTime occurenceStart, DateTime occurenceEnd, CancellationToken cancellationToken = default);
        Task<QuestOccurrence?> GetCurrentOccurrenceForQuestAsync(int questId, DateTime now, CancellationToken cancellationToken = default);
        Task<List<QuestOccurrence>> GetAllOccurrencesForQuestAsync(int questId, CancellationToken cancellationToken = default);
    }
}
