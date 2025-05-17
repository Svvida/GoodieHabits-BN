using Domain.Models;

namespace Application.Interfaces.Quests
{
    public interface IQuestStatisticsService
    {
        Task ProcessOccurrencesAsync(CancellationToken cancellationToken = default);
        public QuestStatistics CalculateStatistics(IEnumerable<QuestOccurrence> occurrences)
        {
            var stats = new QuestStatistics();
            var ordered = occurrences.OrderBy(o => o.OccurenceStart).ToList();

            foreach (var occurence in ordered)
            {
                stats.OccurenceCount++;

                if (occurence.WasCompleted)
                {
                    stats.CompletionCount++;
                    stats.LastCompletedAt = occurence.CompletedAt;
                    stats.CurrentStreak++;

                    if (stats.CurrentStreak > stats.LongestStreak)
                    {
                        stats.LongestStreak = stats.CurrentStreak;
                    }
                }
                else
                {
                    stats.FailureCount++;
                    stats.CurrentStreak = 0;
                }
            }

            return stats;
        }
    }
}
