using Domain.Models;

namespace Domain.Interfaces.Quests
{
    public interface IQuestOccurrenceRepository
    {
        Task SaveOccurrencesAsync(List<QuestOccurrence> occurences, CancellationToken cancellationToken = default);
        Task<bool> IsQuestOccurrenceExistsAsync(int questId, DateTime occurenceStart, DateTime occurenceEnd, CancellationToken cancellationToken = default);
        Task<QuestOccurrence?> GetCurrentOccurrenceForQuestAsync(int questId, DateTime now, CancellationToken cancellationToken = default);
        Task UpdateOccurrence(QuestOccurrence occurence, CancellationToken cancellationToken = default);
    }
}
