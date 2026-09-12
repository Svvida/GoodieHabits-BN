using Domain.Calculators;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.Quests.Queries.GetQuestAnalytics
{
    public class GetQuestAnalyticsQueryHandler(IUnitOfWork unitOfWork, IClock clock)
        : IRequestHandler<GetQuestAnalyticsQuery, GetQuestAnalyticsResponse>
    {
        private const int DefaultWindowDays = 90;

        public async Task<GetQuestAnalyticsResponse> Handle(GetQuestAnalyticsQuery request, CancellationToken cancellationToken)
        {
            // One load: the statistics row comes along rather than being fetched by a second query.
            var quest = await unitOfWork.Quests
                .GetQuestByIdAsync(request.QuestId, request.UserProfileId, asNoTracking: true, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {request.QuestId} not found.");

            if (!quest.Schedule.IsRepeatable)
                throw new InvalidArgumentException("Analytics are only available for repeating quests.");

            var today = quest.UserProfile.LocalDateOn(clock.GetCurrentInstant().ToDateTimeUtc());

            var to = request.To ?? today;
            var from = request.From ?? to.AddDays(-DefaultWindowDays);

            var occurrences = await unitOfWork.QuestOccurrences
                .GetForQuestInRangeAsync(quest.Id, from, to, cancellationToken)
                .ConfigureAwait(false);

            var completions = await unitOfWork.QuestCompletions
                .GetForQuestInRangeAsync(quest.Id, from, to, cancellationToken)
                .ConfigureAwait(false);

            return new GetQuestAnalyticsResponse(
                QuestId: quest.Id,
                Title: quest.Title,
                StreakUnit: quest.Schedule.Unit.ToString(),
                From: from,
                To: to,
                Granularity: request.Granularity.ToString(),
                Range: QuestAnalyticsCalculator.Summarize(occurrences, today, completions.Count),
                Lifetime: ToLifetimeDto(quest.Statistics),
                Calendar: QuestAnalyticsCalculator.ToCalendar(occurrences, today),
                Trend: QuestAnalyticsCalculator.Bucket(occurrences, request.Granularity, today, quest.UserProfile.WeekStartsOn),
                ByWeekday: QuestAnalyticsCalculator.ByWeekday(completions, occurrences, from, to),
                ByHourOfDay: QuestAnalyticsCalculator.ByHourOfDay(completions));
        }

        private static LifetimeQuestStatsDto? ToLifetimeDto(QuestStatistics? statistics)
        {
            if (statistics is null)
                return null;

            int evaluated = statistics.CompletionCount + statistics.FailureCount;

            return new LifetimeQuestStatsDto(
                statistics.CompletionCount,
                statistics.FailureCount,
                statistics.PartialCount,
                statistics.OccurrenceCount,
                statistics.TotalCompletions,
                statistics.CurrentStreak,
                statistics.LongestStreak,
                evaluated == 0 ? null : Math.Round((double)statistics.CompletionCount / evaluated, 4),
                statistics.LastCompletedAt);
        }
    }
}
