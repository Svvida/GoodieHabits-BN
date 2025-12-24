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

            var userTimeZone = DateTimeZoneProviders.Tzdb[userProfile.TimeZone]
                ?? throw new InvalidArgumentException($"Invalid timezone: {userProfile.TimeZone}");

            Instant utcNow = SystemClock.Instance.GetCurrentInstant();
            // Get Local Time
            LocalDateTime localNow = utcNow.InZone(userTimeZone).LocalDateTime;

            // Calculate Search Range
            DateTime todayStart = localNow.Date.AtStartOfDayInZone(userTimeZone).ToDateTimeUtc();
            DateTime todayEnd = todayStart.AddDays(1).AddTicks(-1);

            // Extract the exact Day/Weekday for the User
            var userLocalWeekday = (WeekdayEnum)localNow.DayOfWeek.ToDayOfWeek();
            var userLocalDayOfMonth = localNow.Day;

            logger.LogDebug("Today start: {TodayStart}, Today end: {TodayEnd}",
                todayStart.ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
                todayEnd.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));

            SeasonEnum currentSeason = SeasonHelper.GetCurrentSeason(utcNow.ToDateTimeUtc());

            var quests = await unitOfWork.Quests.GetActiveQuestsForDisplayAsync(
                request.UserProfileId,
                todayStart,
                todayEnd,
                userLocalWeekday,
                userLocalDayOfMonth,
                currentSeason,
                cancellationToken).ConfigureAwait(false);

            return quests.Select(questMappingService.MapToDto);
        }
    }
}
