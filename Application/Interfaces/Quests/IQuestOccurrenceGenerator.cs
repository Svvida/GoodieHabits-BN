using Domain.Models;

namespace Application.Interfaces.Quests
{
    public interface IQuestOccurrenceGenerator
    {
        Task<List<QuestOccurrence>> GenerateMissingOccurrencesAsync(Quest quest, CancellationToken cancellationToken = default);
    }
}
