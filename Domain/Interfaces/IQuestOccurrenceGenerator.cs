using Domain.Models;

namespace Domain.Interfaces
{
    public interface IQuestOccurrenceGenerator
    {
        void GenerateOccurrenceForNewQuest(Quest quest);
        int GenerateMissingOccurrencesForQuest(Quest quest);
    }
}
