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
        private readonly IQuestOccurenceRepository _questOccurenceRepository;
        private readonly IClock _clock;

        public QuestStatisticsService(
            IQuestRepository questRepository,
            IQuestOccurenceRepository questOccurenceRepository,
            IClock clock)
        {
            _questRepository = questRepository;
            _questOccurenceRepository = questOccurenceRepository;
            _clock = clock;
        }

        public async Task ProcessOccurrencesAsync(CancellationToken cancellationToken = default)
        {
            var now = _clock.GetCurrentInstant().ToDateTimeUtc();
            var repeatableQuests = await _questRepository.GetRepeatableQuestsAsync(cancellationToken);
            var occurences = new List<QuestOccurrence>();

            foreach (var quest in repeatableQuests)
            {
                var userZone = DateTimeZoneProviders.Tzdb[quest.Account.TimeZone];
                var lastDate = quest.LastCompletedAt ?? quest.StartDate ?? now;

                List<(DateTime, DateTime)> windows = GenerateExpectedWindows(quest, lastDate, now, userZone);

                foreach (var (startUtc, endUtc) in windows)
                {
                    if (!await _questOccurenceRepository.IsQuestOccurenceExistsAsync(quest.Id, startUtc, endUtc, cancellationToken).ConfigureAwait(false))
                    {
                        var occurrence = new QuestOccurrence
                        {
                            QuestId = quest.Id,
                            OccurenceStart = startUtc,
                            OccurenceEnd = endUtc,
                            WasCompleted = false,
                        };
                        occurences.Add(occurrence);
                    }
                }
                await _questOccurenceRepository.SaveOccurencesAsync(occurences, cancellationToken).ConfigureAwait(false);
            }
        }

        private List<(DateTime startUtc, DateTime endUtc)> GenerateExpectedWindows(Quest quest, DateTime fromUtc, DateTime toUtc, DateTimeZone userZone)
        {
            var windows = new List<(DateTime, DateTime)>();
            var fromLocal = Instant.FromDateTimeUtc(fromUtc).InZone(userZone).Date;
            var toLocal = Instant.FromDateTimeUtc(toUtc).InZone(userZone).Date;

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
                var validDays = quest.WeeklyQuest_Days.Select(d => (DayOfWeek)d.Weekday).ToHashSet();

                for (var date = fromLocal; date <= toLocal; date = date.PlusDays(1))
                {
                    if (validDays.Contains((DayOfWeek)date.DayOfWeek))
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

                for (var ym = startMonth; ym <= endMonth; ym.PlusMonths(1))
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
