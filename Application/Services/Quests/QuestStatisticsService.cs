using Application.Interfaces.Quests;
using Domain.Enum;
using Domain.Interfaces.Quests;
using Domain.Models;
using NodaTime;

namespace Application.Services.Quests
{
    public class QuestStatisticsService : IQuestStatisticsService
    {
        private readonly IQuestRepository _questRepository;
        private readonly IQuestOccurrenceRepository _questOccurrenceRepository;
        private readonly IClock _clock;

        public QuestStatisticsService(
            IQuestRepository questRepository,
            IQuestOccurrenceRepository questOccurrenceRepository,
            IClock clock)
        {
            _questRepository = questRepository;
            _questOccurrenceRepository = questOccurrenceRepository;
            _clock = clock;
        }

        public async Task ProcessOccurrencesAsync(CancellationToken cancellationToken = default)
        {
            var now = _clock.GetCurrentInstant().ToDateTimeUtc();
            var repeatableQuests = await _questRepository.GetRepeatableQuestsAsync(cancellationToken).ConfigureAwait(false);
            var occurences = new List<QuestOccurrence>();

            foreach (var quest in repeatableQuests)
            {
                var userZone = DateTimeZoneProviders.Tzdb[quest.Account.TimeZone];
                var lastDate = quest.LastCompletedAt ?? quest.StartDate ?? now;

                List<(DateTime, DateTime)> windows = GenerateExpectedWindows(quest, lastDate, now, userZone);

                foreach (var (startUtc, endUtc) in windows)
                {
                    if (!await _questOccurrenceRepository.IsQuestOccurrenceExistsAsync(quest.Id, startUtc, endUtc, cancellationToken).ConfigureAwait(false))
                    {
                        var occurrence = new QuestOccurrence
                        {
                            QuestId = quest.Id,
                            OccurrenceStart = startUtc,
                            OccurrenceEnd = endUtc,
                            WasCompleted = false,
                        };
                        occurences.Add(occurrence);
                    }
                }
            }
            await _questOccurrenceRepository.SaveOccurrencesAsync(occurences, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<QuestOccurrence>> ProcessOccurrencesForQuestAsync(Quest quest, CancellationToken cancellationToken = default)
        {
            var now = _clock.GetCurrentInstant().ToDateTimeUtc();
            var userZone = DateTimeZoneProviders.Tzdb[quest.Account.TimeZone];
            var lastDate = quest.LastCompletedAt ?? now;

            var windows = GenerateExpectedWindows(quest, lastDate, now, userZone);
            var newOccurrences = new List<QuestOccurrence>();

            foreach (var (startUtc, endUtc) in windows)
            {
                bool exists = await _questOccurrenceRepository
                    .IsQuestOccurrenceExistsAsync(quest.Id, startUtc, endUtc, cancellationToken)
                    .ConfigureAwait(false);

                if (!exists)
                {
                    newOccurrences.Add(new QuestOccurrence
                    {
                        QuestId = quest.Id,
                        OccurrenceStart = startUtc,
                        OccurrenceEnd = endUtc,
                        WasCompleted = false,
                    });
                }
            }

            if (newOccurrences.Count > 0)
            {
                await _questOccurrenceRepository
                    .SaveOccurrencesAsync(newOccurrences, cancellationToken)
                    .ConfigureAwait(false);
            }

            return newOccurrences;
        }

        public QuestStatistics CalculateStatistics(IEnumerable<QuestOccurrence> occurrences)
        {
            var stats = new QuestStatistics();
            var ordered = occurrences.OrderBy(o => o.OccurrenceStart).ToList();

            foreach (var occurence in ordered)
            {
                stats.OccurrenceCount++;

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

        private static List<(DateTime, DateTime)> GenerateExpectedWindows(Quest quest, DateTime fromUtc, DateTime toUtc, DateTimeZone userZone)
        {
            var windows = new List<(DateTime, DateTime)>();
            var fromLocal = Instant.FromDateTimeUtc(DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc)).InZone(userZone).Date;
            var toLocal = Instant.FromDateTimeUtc(DateTime.SpecifyKind(toUtc, DateTimeKind.Utc)).InZone(userZone).Date;

            if (quest.QuestType == QuestTypeEnum.Daily)
            {
                for (var date = fromLocal; date <= toLocal; date = date.PlusDays(1))
                {
                    var startUtc = date.AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();
                    var endUtc = date.PlusDays(1).AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();
                    windows.Add((startUtc, endUtc));
                }
            }
            else if (quest.QuestType == QuestTypeEnum.Weekly)
            {
                var scheduledWeekdays = quest.WeeklyQuest_Days.Select(d => (DayOfWeek)d.Weekday).ToHashSet();

                for (var date = fromLocal; date <= toLocal; date = date.PlusDays(1))
                {
                    if (scheduledWeekdays.Contains((DayOfWeek)date.DayOfWeek))
                    {
                        var startUtc = date.AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();
                        var endUtc = date.PlusDays(1).AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();
                        windows.Add((startUtc, endUtc));
                    }
                }
            }
            else if (quest.QuestType == QuestTypeEnum.Monthly)
            {
                var startDay = quest.MonthlyQuest_Days!.StartDay;
                var endDay = quest.MonthlyQuest_Days!.EndDay;

                var startMonth = new YearMonth(fromLocal.Year, fromLocal.Month);
                var endMonth = new YearMonth(toLocal.Year, toLocal.Month);

                for (var ym = startMonth; ym <= endMonth; ym = ym.PlusMonths(1))
                {
                    var daysInMonth = ym.ToDateInterval().End.Day;
                    var sDay = Math.Min(startDay, daysInMonth);
                    var eDay = Math.Min(endDay, daysInMonth);

                    var startUtc = ym.OnDayOfMonth(sDay).AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();
                    var endUtc = ym.OnDayOfMonth(eDay).PlusDays(1).AtMidnight().InZoneLeniently(userZone).ToDateTimeUtc();

                    if (endUtc > startUtc)
                        windows.Add((startUtc, endUtc));
                }
            }

            return windows;
        }
    }
}
