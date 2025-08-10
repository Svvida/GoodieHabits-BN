using Domain.Models;

namespace Domain.Interfaces
{
    public interface IQuestStatisticsCalculator
    {
        QuestStatistics Calculate(IEnumerable<QuestOccurrence> occurrences);
    }
}
