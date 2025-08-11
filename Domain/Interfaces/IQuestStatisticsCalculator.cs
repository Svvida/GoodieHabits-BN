using Domain.Models;
using Domain.ValueObjects;

namespace Domain.Interfaces
{
    public interface IQuestStatisticsCalculator
    {
        QuestStatisticsData Calculate(IEnumerable<QuestOccurrence> occurrences);
    }
}
