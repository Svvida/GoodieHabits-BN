using Application.Dtos.Quests;
using Application.Helpers;
using Application.Interfaces.Quests;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Quests.Queries.GetActiveQuests
{
    public class GetActiveQuestsQueryHandler(
        IUnitOfWork unitOfWork,
        IQuestMappingService questMappingService,
        ILogger<GetActiveQuestsQueryHandler> logger)
        : IRequestHandler<GetActiveQuestsQuery, IEnumerable<BaseGetQuestDto>>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IQuestMappingService _questMappingService = questMappingService;
        private readonly ILogger<GetActiveQuestsQueryHandler> _logger = logger;

        public async Task<IEnumerable<BaseGetQuestDto>> Handle(GetActiveQuestsQuery request, CancellationToken cancellationToken = default)
        {
            var account = await _unitOfWork.Accounts.GetByIdAsync(request.AccountId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Account with ID {request.AccountId} not found.");

            var userTimeZone = DateTimeZoneProviders.Tzdb[account.TimeZone]
                ?? throw new InvalidArgumentException($"Invalid timezone: {account.TimeZone}");

            Instant utcNow = SystemClock.Instance.GetCurrentInstant();
            LocalDateTime localNow = utcNow.InZone(userTimeZone).LocalDateTime;

            DateTime todayStart = localNow.Date.AtStartOfDayInZone(userTimeZone).ToDateTimeUtc();
            DateTime todayEnd = todayStart.AddDays(1).AddTicks(-1);

            _logger.LogDebug("Today start: {TodayStart}, Today end: {TodayEnd}",
                todayStart.ToString("yyyy-MM-dd HH:mm:ss.fffffff"),
                todayEnd.ToString("yyyy-MM-dd HH:mm:ss.fffffff"));

            SeasonEnum currentSeason = SeasonHelper.GetCurrentSeason();

            var quests = await _unitOfWork.Quests.GetActiveQuestsForDisplayAsync(request.AccountId, todayStart, todayEnd, currentSeason, cancellationToken).ConfigureAwait(false);

            return quests.Select(_questMappingService.MapToDto);
        }
    }
}
