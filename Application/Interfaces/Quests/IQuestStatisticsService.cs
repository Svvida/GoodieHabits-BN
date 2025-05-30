using Domain.Models;

namespace Application.Interfaces.Quests
{
    public interface IQuestStatisticsService
    {
        Task ProcessOccurrencesAsync(CancellationToken cancellationToken = default);
        Task ProcessStatisticsForQuestsAsync(IEnumerable<Quest> quests, CancellationToken cancellationToken = default);
        Task ProcessStatisticsForQuestAsync(Quest quest, CancellationToken cancellationToken = default);
    }
}
