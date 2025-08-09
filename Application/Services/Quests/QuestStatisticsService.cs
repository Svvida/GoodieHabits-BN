using Application.Common.Interfaces.Quests;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class QuestStatisticsService(
        ILogger<QuestStatisticsService> logger,
        IQuestStatisticsCalculator questStatisticsCalculator,
        IUnitOfWork unitOfWork) : IQuestStatisticsService
    {
        public async Task ProcessStatisticsForQuestAndSaveAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await unitOfWork.Quests.GetQuestForStatsProcessingAsync(questId, cancellationToken).ConfigureAwait(false);

            if (quest is not null)
                await ProcessStatisticsForQuestAsync(quest, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<int> ProcessStatisticsForQuestsAndSaveAsync(CancellationToken cancellationToken = default)
        {
            var repeatableQuests = await unitOfWork.Quests.GetRepeatableQuestsForStatsProcessingAsync(cancellationToken);

            foreach (var quest in repeatableQuests)
            {
                await ProcessStatisticsForQuestAsync(quest, cancellationToken).ConfigureAwait(false);
            }

            return await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task ProcessStatisticsForQuestAsync(Quest quest, CancellationToken cancellationToken = default)
        {
            var occurrences = await unitOfWork.QuestOccurrences.GetAllOccurrencesForQuestAsync(quest.Id, cancellationToken);
            var newStats = questStatisticsCalculator.Calculate(occurrences);

            logger.LogDebug("Processing statistics for quest {QuestId}: {@NewStats}", quest.Id, new
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


            UpdateQuestStatistics(quest, newStats);

            logger.LogDebug("Updated quest statistics for quest {QuestId}: {@QuestStatsData}", quest.Id, new
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