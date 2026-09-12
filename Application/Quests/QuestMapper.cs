using Application.QuestLabels.Dtos;
using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Models;
using Domain.ValueObjects;
using MapsterMapper;

namespace Application.Quests
{
    /// <summary>
    /// Projects quests for the API.
    /// <para>
    /// Hand-written rather than a Mapster profile because the interesting fields are not projections of
    /// columns: completion, the current period and "at risk" are all derived from the user's local today,
    /// which a declarative mapping has no way to receive. Labels still go through Mapster.
    /// </para>
    /// </summary>
    public class QuestMapper(IMapper mapper) : IQuestMapper
    {
        public QuestDetailsDto MapToDto(Quest quest, DateOnly today, bool includeLegacyType = false)
        {
            return new QuestDetailsDto
            {
                Id = quest.Id,
                Title = quest.Title,
                Description = quest.Description,
                StartDate = quest.StartDate,
                EndDate = quest.EndDate,
                Emoji = quest.Emoji,
                Priority = quest.Priority?.ToString(),
                Difficulty = quest.Difficulty?.ToString(),
                ScheduledTime = quest.ScheduledTime,
                DurationMinutes = quest.DurationMinutes,
                Schedule = ToDto(quest.Schedule),
                Target = ToDto(quest.Target),
                IsCompleted = quest.IsCompletedOn(today),
                CurrentPeriod = ToCurrentPeriodDto(quest, today),
                LastCompletedAt = quest.LastCompletedAt,
                Statistics = ToDto(quest.Statistics),
                Labels = [.. quest.Quest_QuestLabels.Select(mapper.Map<QuestLabelDto>)],
                LegacyQuestType = includeLegacyType ? LegacyQuestTypeOf(quest.Schedule).ToString() : null
            };
        }

        public static QuestScheduleDto ToDto(QuestSchedule schedule) => new(
            Unit: schedule.Unit.ToString(),
            Interval: schedule.Interval,
            Weekdays: schedule.Weekdays?.ToWeekdays().Select(w => w.ToString()).ToList(),
            MonthWindowStartDay: schedule.MonthWindowStartDay,
            MonthWindowEndDay: schedule.MonthWindowEndDay,
            YearWindowStart: schedule.YearWindowStart,
            YearWindowEnd: schedule.YearWindowEnd);

        public static QuestTargetDto ToDto(QuestTarget target) => new(
            Amount: target.Amount,
            Unit: target.Unit,
            Mode: target.Mode.ToString(),
            MaxCompletionsPerDay: target.MaxCompletionsPerDay);

        public static RepeatableQuestStatisticsDto? ToDto(QuestStatistics? statistics) =>
            statistics is null
                ? null
                : new RepeatableQuestStatisticsDto(
                    statistics.CompletionCount,
                    statistics.FailureCount,
                    statistics.PartialCount,
                    statistics.OccurrenceCount,
                    statistics.TotalCompletions,
                    statistics.CurrentStreak,
                    statistics.LongestStreak,
                    statistics.LastCompletedAt);

        /// <summary>
        /// Today's period as the client should see it. Falls back to the *computed* window when nothing has
        /// been materialized yet, so a read never has to write to answer "what am I doing today".
        /// </summary>
        private static CurrentPeriodDto? ToCurrentPeriodDto(Quest quest, DateOnly today)
        {
            var period = quest.CurrentPeriod(today);

            if (period is not null)
            {
                return BuildCurrentPeriodDto(
                    period.PeriodStart, period.PeriodEnd, period.Progress, period.TargetAmount,
                    period.OutcomeOn(today), today, quest.Target.MaxCompletionsPerDay,
                    period.Completions);
            }

            if (quest.PeriodWindowOn(today) is not QuestPeriodWindow window)
                return null;

            // Not yet materialized: it will be, by the first completion or the maintenance pass. Nothing can
            // have been recorded against a period that does not exist, so the tap list is empty by definition.
            return BuildCurrentPeriodDto(
                window.Start, window.End, 0m, quest.Target.ProratedAmount(window.Days, window.FullDays),
                QuestPeriodOutcomeEnum.Pending, today, quest.Target.MaxCompletionsPerDay,
                []);
        }

        private static CurrentPeriodDto BuildCurrentPeriodDto(
            DateOnly start,
            DateOnly end,
            decimal progress,
            decimal target,
            QuestPeriodOutcomeEnum outcome,
            DateOnly today,
            int? maxPerDay,
            IEnumerable<QuestCompletion> completions)
        {
            decimal remaining = Math.Max(0m, target - progress);
            int remainingDays = Math.Max(0, end.DayNumber - today.DayNumber + 1);

            var ordered = completions
                .OrderBy(c => c.CompletedOn)
                .ThenBy(c => c.CompletedAt)
                .ToList();

            var todaysCompletions = ordered.Where(c => c.CompletedOn == today).ToList();

            return new CurrentPeriodDto(
                Start: start,
                End: end,
                Progress: progress,
                Target: target,
                Remaining: remaining,
                Outcome: outcome.ToString(),
                RemainingDays: remainingDays,
                IsAtRisk: IsAtRisk(remaining, remainingDays, maxPerDay, isSingleDayPeriod: start == end),
                TodayProgress: todaysCompletions.Sum(c => c.Amount),
                CanCompleteToday: maxPerDay is not int cap || todaysCompletions.Count < cap,
                Completions: [.. ordered.Select(c => new PeriodCompletionDto(c.Id, c.CompletedOn, c.Amount))]);
        }

        /// <summary>
        /// Still achievable, but only just: what is left needs every remaining day.
        /// <para>
        /// Never true for a single-day period. There, "not done yet" is the normal state of every habit
        /// before the user gets to it, and flagging all of them as at risk says nothing. The signal is only
        /// meaningful when there were several days to spread the work over and they are running out.
        /// </para>
        /// </summary>
        private static bool IsAtRisk(decimal remaining, int remainingDays, int? maxPerDay, bool isSingleDayPeriod)
        {
            if (isSingleDayPeriod || remaining <= 0m || remainingDays <= 0)
                return false;

            decimal perDay = maxPerDay ?? remaining;

            return remaining > perDay * (remainingDays - 1);
        }

        /// <summary>
        /// Best-effort mapping back onto the retired <c>QuestTypeEnum</c>, for the compatibility routes only.
        /// Schedules the old model could not express (a Week unit, an interval above 1) land on the nearest
        /// type, which is why the shim is temporary rather than permanent.
        /// </summary>
        public static QuestTypeEnum LegacyQuestTypeOf(QuestSchedule schedule) => schedule.Unit switch
        {
            PeriodUnitEnum.None => QuestTypeEnum.OneTime,
            PeriodUnitEnum.Day => schedule.Weekdays.HasValue ? QuestTypeEnum.Weekly : QuestTypeEnum.Daily,
            PeriodUnitEnum.Week => QuestTypeEnum.Weekly,
            PeriodUnitEnum.Month => QuestTypeEnum.Monthly,
            PeriodUnitEnum.Year => QuestTypeEnum.Seasonal,
            _ => QuestTypeEnum.OneTime
        };
    }
}
