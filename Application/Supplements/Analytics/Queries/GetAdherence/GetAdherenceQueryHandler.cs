using Application.Supplements.Analytics.Dtos;
using Domain.Calculators;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Supplements.Analytics.Queries.GetAdherence
{
    /// <summary>
    /// Planned doses versus doses actually logged, over a range.
    /// <para>
    /// "Today" comes from <c>UserProfile.LocalDateOn</c>, not from UTC — the elapsed-day denominator is a
    /// statement about the user's calendar, and getting it from the server's clock would mark a day missed
    /// hours early (or late) for anyone in another timezone.
    /// </para>
    /// </summary>
    public class GetAdherenceQueryHandler(IUnitOfWork unitOfWork, IClock clock)
        : IRequestHandler<GetAdherenceQuery, SupplementAdherenceReportDto>
    {
        public async Task<SupplementAdherenceReportDto> Handle(GetAdherenceQuery request, CancellationToken cancellationToken)
        {
            var profile = await unitOfWork.UserProfiles
                .GetByIdAsync(request.UserProfileId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"User profile with ID {request.UserProfileId} not found.");

            var today = profile.LocalDateOn(clock.GetCurrentInstant().ToDateTimeUtc());

            var supplements = await unitOfWork.Supplements
                .GetUserSupplementsAsync(request.UserProfileId, false, cancellationToken)
                .ConfigureAwait(false);

            var intakes = await unitOfWork.SupplementIntakes
                .GetForPeriodAsync(request.UserProfileId, request.From, request.To, cancellationToken)
                .ConfigureAwait(false);

            var items = new List<SupplementAdherenceItemDto>();

            foreach (var supplement in supplements)
            {
                var slotIds = supplement.Slots.Select(s => s.Id).ToHashSet();

                if (slotIds.Count == 0)
                    continue;

                // Ad-hoc doses are excluded from the numerator on purpose: they were never scheduled, so
                // counting them would let unplanned doses paper over a plan that isn't being followed.
                var planned = intakes
                    .Where(i => i.ScheduleSlotId is int slotId && slotIds.Contains(slotId))
                    .ToList();

                var daysWithIntake = planned.Select(i => i.TakenOn).ToHashSet();

                var evaluatedDays = SupplementAdherenceCalculator
                    .CountEvaluatedDays(request.From, request.To, today, daysWithIntake);

                var adherence = SupplementAdherenceCalculator
                    .Calculate(slotIds.Count, evaluatedDays, planned.Count);

                items.Add(new SupplementAdherenceItemDto
                {
                    SupplementId = supplement.Id,
                    SupplementName = supplement.Name,
                    SlotsPerDay = slotIds.Count,
                    Scheduled = adherence.Scheduled,
                    Taken = adherence.Taken,
                    Rate = adherence.Rate,
                });
            }

            var overallScheduled = items.Sum(i => i.Scheduled);
            var overallTaken = items.Sum(i => i.Taken);
            var overall = SupplementAdherenceCalculator.Calculate(1, overallScheduled, overallTaken);

            return new SupplementAdherenceReportDto
            {
                From = request.From,
                To = request.To,
                Scheduled = overallScheduled,
                Taken = overallTaken,
                Rate = overall.Rate,
                Items = [.. items.OrderBy(i => i.SupplementName)],
            };
        }
    }
}
