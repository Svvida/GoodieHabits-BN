using Domain.Models;

namespace Application.Interfaces.Quests
{
    public interface IQuestStatisticsService
    {
        Task ProcessOccurrencesAsync(CancellationToken cancellationToken = default);
        Task<List<QuestOccurrence>> ProcessOccurrencesForQuestAsync(Quest quest, CancellationToken cancellationToken = default);
        QuestStatistics CalculateStatistics(IEnumerable<QuestOccurrence> occurrences);
    }
}
