using Domain.Models;

namespace Application.Interfaces.Quests
{
    public interface IQuestOccurrenceGenerator
    {
        Task<List<QuestOccurrence>> GenerateMissingOccurrencesForQuestAsync(Quest quest, CancellationToken cancellationToken = default);
        Task<int> GenerateAndSaveMissingOccurrencesForQuestsAsync(CancellationToken cancellationToken = default);
    }
}
