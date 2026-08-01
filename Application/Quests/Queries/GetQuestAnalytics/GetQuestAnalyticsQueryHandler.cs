using Domain.Calculators;
using Domain.Exceptions;
using Domain.Interfaces;
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
            var quest = await unitOfWork.Quests
                .GetQuestWithUserProfileAsync(request.QuestId, request.UserProfileId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {request.QuestId} not found.");

            if (!quest.IsRepeatable())
                throw new InvalidArgumentException("Analytics are only available for repeatable quests (Daily, Weekly, Monthly).");

            var today = quest.UserProfile.LocalDateOn(clock.GetCurrentInstant().ToDateTimeUtc());

            var to = request.To ?? today;
            var from = request.From ?? to.AddDays(-DefaultWindowDays);

            var occurrences = await unitOfWork.QuestOccurrences
                .GetForQuestInRangeAsync(quest.Id, from, to, cancellationToken)
                .ConfigureAwait(false);

            var statistics = await unitOfWork.Quests
                .GetQuestByIdAsync(quest.Id, request.UserProfileId, quest.QuestType, asNoTracking: true, cancellationToken)
                .ConfigureAwait(false);

            return new GetQuestAnalyticsResponse(
                QuestId: quest.Id,
                QuestType: quest.QuestType.ToString(),
                Title: quest.Title,
                From: from,
                To: to,
                Granularity: request.Granularity.ToString(),
                Range: QuestAnalyticsCalculator.Summarize(occurrences, today),
                Lifetime: ToLifetimeDto(statistics?.Statistics),
                Calendar: QuestAnalyticsCalculator.ToCalendar(occurrences, today),
                Trend: QuestAnalyticsCalculator.Bucket(occurrences, request.Granularity, today),
                ByWeekday: QuestAnalyticsCalculator.ByWeekday(occurrences, today));
        }

        private static LifetimeQuestStatsDto? ToLifetimeDto(Domain.Models.QuestStatistics? statistics)
        {
            if (statistics is null)
                return null;

            int evaluated = statistics.CompletionCount + statistics.FailureCount;

            return new LifetimeQuestStatsDto(
                statistics.CompletionCount,
                statistics.FailureCount,
                statistics.OccurrenceCount,
                statistics.CurrentStreak,
                statistics.LongestStreak,
                evaluated == 0 ? null : Math.Round((double)statistics.CompletionCount / evaluated, 4),
                statistics.LastCompletedAt);
        }
    }
}
