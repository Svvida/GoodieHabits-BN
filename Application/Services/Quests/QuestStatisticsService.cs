using Application.Interfaces.Quests;
using Domain.Interfaces.Authentication;
using Domain.Interfaces.Quests;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class QuestStatisticsService : IQuestStatisticsService
    {
        private readonly IQuestRepository _questRepository;
        private readonly IQuestOccurrenceRepository _questOccurrenceRepository;
        private readonly IQuestOccurrenceGenerator _occurrenceGenerator;
        private readonly ILogger<QuestStatisticsService> _logger;
        private readonly IQuestStatisticsCalculator _questStatisticsCalculator;
        private readonly IQuestStatisticsRepository _questStatisticsRepository;

        public QuestStatisticsService(
            IQuestRepository questRepository,
            IQuestOccurrenceRepository questOccurrenceRepository,
            IQuestOccurrenceGenerator occurrenceGenerator,
            ILogger<QuestStatisticsService> logger,
            IQuestStatisticsCalculator questStatisticsCalculator,
            IQuestStatisticsRepository questStatisticsRepository)
        {
            _questRepository = questRepository;
            _questOccurrenceRepository = questOccurrenceRepository;
            _occurrenceGenerator = occurrenceGenerator;
            _logger = logger;
            _questStatisticsCalculator = questStatisticsCalculator;
            _questStatisticsRepository = questStatisticsRepository;
        }

        public async Task ProcessOccurrencesAsync(CancellationToken cancellationToken = default)
        {
            var repeatableQuests = await _questRepository.GetRepeatableQuestsAsync(cancellationToken);

            foreach (var quest in repeatableQuests)
            {
                var newOccurrences = await _occurrenceGenerator.GenerateMissingOccurrencesAsync(quest, cancellationToken);
                _logger.LogDebug("Processed {Count} new occurrences for quest {QuestId}", newOccurrences.Count, quest.Id);
            }
        }

        public async Task ProcessStatisticsForQuestsAsync(IEnumerable<Quest> quests, CancellationToken cancellationToken = default)
        {
            foreach (var quest in quests)
            {
                await ProcessStatisticsForQuestAsync(quest, cancellationToken);
            }
        }

        public async Task ProcessStatisticsForQuestAsync(Quest quest, CancellationToken cancellationToken = default)
        {
            var occurrences = await _questOccurrenceRepository.GetAllOccurrencesForQuestAsync(quest.Id, cancellationToken);
            var newStats = _questStatisticsCalculator.Calculate(occurrences);

            _logger.LogDebug("Processing statistics for quest {QuestId}: {@NewStats}", quest.Id, newStats);

            UpdateQuestStatistics(quest, newStats);

            _logger.LogDebug("Updated quest statistics for quest {QuestId}: {@QuestStatsData}", quest.Id, new
            {
                // Select only the properties you need from quest.Statistics
                quest.Statistics!.Id,
                quest.Statistics.QuestId, // This is just an ID, not the full object
                quest.Statistics.CompletionCount,
                quest.Statistics.FailureCount,
                quest.Statistics.OccurrenceCount,
                quest.Statistics.CurrentStreak,
                quest.Statistics.LongestStreak,
                quest.Statistics.LastCompletedAt
            });

            await _questStatisticsRepository.UpdateQuestStatisticsAsync(quest.Statistics, cancellationToken);
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