using Application.Common.Interfaces.Quests;
using Domain.Common;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Services.Quests
{
    public class QuestOccurrencesGenerator(
        IClock clock,
        ILogger<QuestOccurrencesGenerator> logger,
        IUnitOfWork unitOfWork) : IQuestOccurrenceGenerator
    {
        public async Task<List<QuestOccurrence>> GenerateMissingOccurrencesForQuestAsync(Quest quest, CancellationToken cancellationToken = default)
        {
            var now = clock.GetCurrentInstant().ToDateTimeUtc();
            var lastDate = quest.LastCompletedAt ?? quest.StartDate ?? quest.CreatedAt;

            var windows = QuestWindowCalculator.GenerateWindows(quest, lastDate, now);
            var newOccurrences = new List<QuestOccurrence>();

            foreach (var window in windows)
            {
                bool exists = await unitOfWork.QuestOccurrences
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
            var now = clock.GetCurrentInstant().ToDateTimeUtc();
            // Fetch all active repeatable quests with their occurrences
            var repeatableQuests = await unitOfWork.Quests.GetRepeatableQuestsForOccurrencesProcessingAsync(now, cancellationToken);

            logger.LogDebug("Found {Count} repeatable quests to process.", repeatableQuests.Count());

            var allNewOcurrencesToSave = new List<QuestOccurrence>();

            foreach (var quest in repeatableQuests)
            {
                var lastDate = quest.LastCompletedAt ?? quest.StartDate ?? quest.CreatedAt;
                var windows = QuestWindowCalculator.GenerateWindows(quest, lastDate, now);

                // Use HasSet for better performance when checking for existing occurrences
                var existingQuestOccurrences = new HashSet<(DateTime Start, DateTime End)>(
                    quest.QuestOccurrences.Select(qo => (qo.OccurrenceStart, qo.OccurrenceEnd)));

                var newOccurrencesForThisQuest = new List<QuestOccurrence>();

                foreach (var window in windows)
                {
                    if (!existingQuestOccurrences.Contains((window.Start, window.End)))
                    {
                        newOccurrencesForThisQuest.Add(CreateOccurrence(quest.Id, window));
                    }
                }

                if (newOccurrencesForThisQuest.Count != 0)
                {
                    logger.LogDebug("Generated {Count} new occurrences for quest {QuestId}", newOccurrencesForThisQuest.Count, quest.Id);
                    allNewOcurrencesToSave.AddRange(newOccurrencesForThisQuest);
                }
            }

            if (allNewOcurrencesToSave.Count == 0)
            {
                logger.LogInformation("No new occurrences needed to be generated for any quests.");
                return 0;
            }

            await unitOfWork.QuestOccurrences.AddRangeAsync(allNewOcurrencesToSave, cancellationToken).ConfigureAwait(false);

            int affectedRows = await unitOfWork.SaveChangesAsync(cancellationToken);
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
