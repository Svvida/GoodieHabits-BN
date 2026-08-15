using Domain.Calculators;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.Quests.Queries.GetHabitsOverview
{
    public class GetHabitsOverviewQueryHandler(IUnitOfWork unitOfWork, IClock clock)
        : IRequestHandler<GetHabitsOverviewQuery, GetHabitsOverviewResponse>
    {
        private const int DefaultWindowDays = 30;

        public async Task<GetHabitsOverviewResponse> Handle(GetHabitsOverviewQuery request, CancellationToken cancellationToken)
        {
            var userProfile = await unitOfWork.UserProfiles.GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Profile with ID: {request.UserProfileId} not found.");

            var today = userProfile.LocalDateOn(clock.GetCurrentInstant().ToDateTimeUtc());

            var to = request.To ?? today;
            var from = request.From ?? to.AddDays(-DefaultWindowDays);

            var occurrences = await unitOfWork.QuestOccurrences
                .GetForUserInRangeAsync(request.UserProfileId, from, to, cancellationToken)
                .ConfigureAwait(false);

            // Group by the id, not the navigation property: the query is AsNoTracking without identity
            // resolution, so each occurrence row carries its own Quest instance and Quest uses reference
            // equality — grouping on it would yield one group per occurrence instead of one per quest.
            var perQuest = occurrences
                .GroupBy(o => o.QuestId)
                .Select(group =>
                {
                    var quest = group.First().Quest;
                    return new HabitSummaryDto(
                        quest.Id,
                        quest.QuestType.ToString(),
                        quest.Title,
                        quest.Emoji,
                        QuestAnalyticsCalculator.Summarize(group, today));
                })
                .OrderByDescending(summary => summary.Summary.CompletionRate ?? -1)
                .ThenBy(summary => summary.Title)
                .ToList();

            return new GetHabitsOverviewResponse(
                From: from,
                To: to,
                Overall: QuestAnalyticsCalculator.Summarize(occurrences, today),
                Quests: perQuest,
                DailyCompletionRate: BuildDailySeries(occurrences, from, to, today));
        }

        /// <summary>
        /// Expands each period across the days it covers so a monthly habit contributes to every day of
        /// its window, then reports one point per calendar day. Days with nothing scheduled are omitted.
        /// </summary>
        private static List<DailyCompletionRateDto> BuildDailySeries(
            IReadOnlyCollection<QuestOccurrence> occurrences,
            DateOnly from,
            DateOnly to,
            DateOnly today)
        {
            var completedByDay = new Dictionary<DateOnly, int>();
            var evaluatedByDay = new Dictionary<DateOnly, int>();

            foreach (var occurrence in occurrences)
            {
                var outcome = QuestAnalyticsCalculator.OutcomeOf(occurrence, today);
                if (outcome == QuestPeriodOutcomeEnum.Pending)
                    continue;

                var start = occurrence.PeriodStart < from ? from : occurrence.PeriodStart;
                var end = occurrence.PeriodEnd > to ? to : occurrence.PeriodEnd;

                for (var day = start; day <= end; day = day.AddDays(1))
                {
                    evaluatedByDay[day] = evaluatedByDay.GetValueOrDefault(day) + 1;

                    if (outcome == QuestPeriodOutcomeEnum.Completed)
                        completedByDay[day] = completedByDay.GetValueOrDefault(day) + 1;
                }
            }

            return [.. evaluatedByDay
                .OrderBy(entry => entry.Key)
                .Select(entry =>
                {
                    int completed = completedByDay.GetValueOrDefault(entry.Key);
                    return new DailyCompletionRateDto(
                        entry.Key,
                        completed,
                        entry.Value,
                        entry.Value == 0 ? null : Math.Round((double)completed / entry.Value, 4));
                })];
        }
    }
}
