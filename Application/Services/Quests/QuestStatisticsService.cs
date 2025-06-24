using Application.Interfaces.Quests;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class QuestStatisticsService : IQuestStatisticsService
    {
        private readonly ILogger<QuestStatisticsService> _logger;
        private readonly IQuestStatisticsCalculator _questStatisticsCalculator;
        private readonly IUnitOfWork _unitOfWork;

        public QuestStatisticsService(
            ILogger<QuestStatisticsService> logger,
            IQuestStatisticsCalculator questStatisticsCalculator,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _questStatisticsCalculator = questStatisticsCalculator;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> ProcessStatisticsForQuestsAndSaveAsync(CancellationToken cancellationToken = default)
        {
            var repeatableQuests = await _unitOfWork.Quests.GetRepeatableQuestsAsync(cancellationToken);

            foreach (var quest in repeatableQuests)
            {
                await ProcessStatisticsForQuestAsync(quest, cancellationToken);
            }

            return await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task ProcessStatisticsForQuestAsync(Quest quest, CancellationToken cancellationToken = default)
        {
            var occurrences = await _unitOfWork.QuestOccurrences.GetAllOccurrencesForQuestAsync(quest.Id, cancellationToken);
            var newStats = _questStatisticsCalculator.Calculate(occurrences);

            _logger.LogDebug("Processing statistics for quest {QuestId}: {@NewStats}", quest.Id, newStats);

            UpdateQuestStatistics(quest, newStats);

            _logger.LogDebug("Updated quest statistics for quest {QuestId}: {@QuestStatsData}", quest.Id, new
            {
                quest.Statistics!.Id,
                quest.Statistics.QuestId,
                quest.Statistics.CompletionCount,
                quest.Statistics.FailureCount,
                quest.Statistics.OccurrenceCount,
                quest.Statistics.CurrentStreak,
                quest.Statistics.LongestStreak,
                quest.Statistics.LastCompletedAt
            });
        }

        private static void UpdateQuestStatistics(Quest quest, QuestStatistics newStats)
        {
            if (quest.Statistics == null)
            {
                quest.Statistics = newStats;
            }
            else
            {
                quest.Statistics.UpdateFrom(newStats);
            }
        }
    }
}