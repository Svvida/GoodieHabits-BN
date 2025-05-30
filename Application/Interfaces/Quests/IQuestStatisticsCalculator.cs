using Domain.Models;

namespace Application.Interfaces.Quests
{
    public interface IQuestStatisticsCalculator
    {
        QuestStatistics Calculate(IEnumerable<QuestOccurrence> occurrences);
    }
}
