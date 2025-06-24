using Application.Interfaces.Quests;
using Application.Models;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Services.Quests
{
    public class QuestOccurrencesGenerator : IQuestOccurrenceGenerator
    {
        private readonly IClock _clock;
        private readonly ILogger<QuestOccurrencesGenerator> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public QuestOccurrencesGenerator(
            IClock clock,
            ILogger<QuestOccurrencesGenerator> logger,
            IUnitOfWork unitOfWork)
        {
            _clock = clock;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<QuestOccurrence>> GenerateMissingOccurrencesForQuestAsync(Quest quest, CancellationToken cancellationToken = default)
        {
            var now = _clock.GetCurrentInstant().ToDateTimeUtc();
            var lastDate = quest.LastCompletedAt ?? now;

            var windows = QuestWindowCalculator.GenerateWindows(quest, lastDate, now);
            var newOccurrences = new List<QuestOccurrence>();

            foreach (var window in windows)
            {
                bool exists = await _unitOfWork.QuestOccurrences
                    .IsQuestOccurrenceExistsAsync(quest.Id, window.Start, window.End, cancellationToken);

                if (!exists)
                {
                    newOccurrences.Add(CreateOccurrence(quest.Id, window));
                }
            }

            return newOccurrences;
        }

        public async Task<int> GenerateAndSaveMissingOccurrencesForQuestsAsync(CancellationToken cancellationToken = default)
        {
            var repeatableQuests = await _unitOfWork.Quests.GetRepeatableQuestsAsync(cancellationToken);
            _logger.LogDebug("Found {Count} repeatable quests to process.", repeatableQuests.Count());

            List<QuestOccurrence> allNewOccurrences = [];
            foreach (var quest in repeatableQuests)
            {
                var newOccurrencesForQuest = await GenerateMissingOccurrencesForQuestAsync(quest, cancellationToken);
                if (newOccurrencesForQuest.Count != 0)
                {
                    _logger.LogDebug("Generated {Count} new occurrences for quest {QuestId}", newOccurrencesForQuest.Count, quest.Id);
                    allNewOccurrences.AddRange(newOccurrencesForQuest);
                }
            }

            if (allNewOccurrences.Count == 0)
            {
                _logger.LogInformation("No new occurrences needed to be generated for any quests.");
                return 0;
            }

            await _unitOfWork.QuestOccurrences.AddRangeAsync(allNewOccurrences, cancellationToken).ConfigureAwait(false);

            int affectedRows = await _unitOfWork.SaveChangesAsync(cancellationToken);
            return affectedRows;
        }

        private static QuestOccurrence CreateOccurrence(int questId, TimeWindow window)
        {
            return new QuestOccurrence
            {
                Id = 0, // New occurrences should have ID 0 before saving
                QuestId = questId,
                OccurrenceStart = window.Start,
                OccurrenceEnd = window.End,
                WasCompleted = false,
            };
        }
    }
}
