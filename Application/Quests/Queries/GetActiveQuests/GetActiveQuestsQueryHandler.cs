using Application.Quests.Dtos;
using Application.Quests.Utilities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Extensions;

namespace Application.Quests.Queries.GetActiveQuests
{
    public class GetActiveQuestsQueryHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService,
        ILogger<GetActiveQuestsQueryHandler> logger)
        : IRequestHandler<GetActiveQuestsQuery, IEnumerable<QuestDetailsDto>>
    {
        public async Task<IEnumerable<QuestDetailsDto>> Handle(GetActiveQuestsQuery request, CancellationToken cancellationToken = default)
        {
            var userProfile = await unitOfWork.UserProfiles.GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {request.UserProfileId} not found.");

            Instant utcNow = SystemClock.Instance.GetCurrentInstant();

            // A quest's active range is a calendar fact, so the whole query works off the user's local date.
            DateOnly today = userProfile.LocalDateOn(utcNow.ToDateTimeUtc());

            var userLocalWeekday = (WeekdayEnum)today.DayOfWeek;
            var userLocalDayOfMonth = today.Day;

            logger.LogDebug("Resolving active quests for user {UserProfileId} on local date {Today}.",
                request.UserProfileId, today);

            SeasonEnum currentSeason = SeasonHelper.GetCurrentSeason(today);

            var quests = await unitOfWork.Quests.GetActiveQuestsForDisplayAsync(
                request.UserProfileId,
                today,
                userLocalWeekday,
                userLocalDayOfMonth,
                currentSeason,
                cancellationToken).ConfigureAwait(false);

            return quests.Select(questMappingService.MapToDto);
        }
    }
}
