using Domain.Models;

namespace Domain.Interfaces.Quests
{
    public interface IQuestOccurenceRepository
    {
        Task SaveOccurencesAsync(List<QuestOccurrence> occurences, CancellationToken cancellationToken = default);
        Task<bool> IsQuestOccurenceExistsAsync(int questId, DateTime occurenceStart, DateTime occurenceEnd, CancellationToken cancellationToken = default);
        Task<QuestOccurrence?> GetCurrentOccurenceForQuestAsync(int questId, DateTime now, CancellationToken cancellationToken = default);
        Task UpdateOccurence(QuestOccurrence occurence, CancellationToken cancellationToken = default);
    }
}
