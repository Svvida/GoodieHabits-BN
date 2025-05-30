using Application.Interfaces.Quests;
using Application.Models;
using Domain.Interfaces.Quests;
using Domain.Models;
using NodaTime;

namespace Application.Services.Quests
{
    public class QuestOccurrenceGenerator : IQuestOccurrenceGenerator
    {
        private readonly IQuestOccurrenceRepository _questOccurrenceRepository;
        private readonly IClock _clock;

        public QuestOccurrenceGenerator(
            IQuestOccurrenceRepository questOccurrenceRepository,
            IClock clock)
        {
            _questOccurrenceRepository = questOccurrenceRepository;
            _clock = clock;
        }

        public async Task<List<QuestOccurrence>> GenerateMissingOccurrencesAsync(Quest quest, CancellationToken cancellationToken = default)
        {
            var now = _clock.GetCurrentInstant().ToDateTimeUtc();
            var lastDate = quest.LastCompletedAt ?? now;

            var windows = QuestWindowCalculator.GenerateWindows(quest, lastDate, now);
            var newOccurrences = new List<QuestOccurrence>();

            foreach (var window in windows)
            {
                bool exists = await _questOccurrenceRepository
                    .IsQuestOccurrenceExistsAsync(quest.Id, window.Start, window.End, cancellationToken);

                if (!exists)
                {
                    newOccurrences.Add(CreateOccurrence(quest.Id, window));
                }
            }

            if (newOccurrences.Count > 0)
            {
                await _questOccurrenceRepository.SaveOccurrencesAsync(newOccurrences, cancellationToken);
            }

            return newOccurrences;
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
