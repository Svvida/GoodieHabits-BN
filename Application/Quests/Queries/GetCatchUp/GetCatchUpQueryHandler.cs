using Application.Quests.Dtos;
using Application.Quests.Services;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.Quests.Queries.GetCatchUp
{
    public class GetCatchUpQueryHandler(
        IUnitOfWork unitOfWork,
        IUserMaintenanceService maintenanceService,
        IClock clock)
        : IRequestHandler<GetCatchUpQuery, GetCatchUpResponse>
    {
        public async Task<GetCatchUpResponse> Handle(GetCatchUpQuery request, CancellationToken cancellationToken)
        {
            // Without this a missed day may not exist as a row yet, and the user would never be asked about it.
            await maintenanceService.EnsureMaintainedAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false);

            var userProfile = await unitOfWork.UserProfiles.GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Profile with ID {request.UserProfileId} not found.");

            var today = userProfile.LocalDateOn(clock.GetCurrentInstant().ToDateTimeUtc());
            var earliest = today.AddDays(-Quest.BackfillGraceDays);

            // Only elapsed periods: a period still running is not a missed tap, it is just today's work.
            var candidates = await unitOfWork.QuestOccurrences
                .GetCatchUpCandidatesAsync(request.UserProfileId, earliest, today.AddDays(-1), request.IncludeCompleted, cancellationToken)
                .ConfigureAwait(false);

            var days = new List<CatchUpDayDto>();

            for (var date = earliest; date < today; date = date.AddDays(1))
            {
                var quests = candidates
                    .Where(period => period.Covers(date))
                    .OrderBy(period => period.Quest.Title)
                    .Select(period => new CatchUpQuestDto(
                        period.QuestId,
                        period.Quest.Title,
                        period.Quest.Emoji,
                        period.PeriodStart,
                        period.PeriodEnd,
                        period.Progress,
                        period.TargetAmount,
                        period.OutcomeOn(today).ToString(),
                        [.. period.Completions
                            .OrderBy(c => c.CompletedOn)
                            .ThenBy(c => c.CompletedAt)
                            .Select(c => new PeriodCompletionDto(c.Id, c.CompletedOn, c.Amount))]))
                    .ToList();

                if (quests.Count > 0)
                    days.Add(new CatchUpDayDto(date, quests));
            }

            return new GetCatchUpResponse(Quest.BackfillGraceDays, days);
        }
    }
}
