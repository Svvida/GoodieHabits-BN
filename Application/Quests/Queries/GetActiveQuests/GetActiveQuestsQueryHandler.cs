using Application.Quests.Dtos;
using Application.Quests.Services;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Quests.Queries.GetActiveQuests
{
    /// <summary>
    /// Today's list. Also the hook the per-user maintenance pass hangs off: this is the call the app makes
    /// on open, so it is where derived state gets brought up to date on hosting that cannot run a timer.
    /// </summary>
    public class GetActiveQuestsQueryHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMapper,
        IUserMaintenanceService maintenanceService,
        IClock clock,
        ILogger<GetActiveQuestsQueryHandler> logger)
        : IRequestHandler<GetActiveQuestsQuery, IEnumerable<QuestDetailsDto>>
    {
        public async Task<IEnumerable<QuestDetailsDto>> Handle(GetActiveQuestsQuery request, CancellationToken cancellationToken = default)
        {
            await maintenanceService.EnsureMaintainedAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false);

            var userProfile = await unitOfWork.UserProfiles.GetByIdAsync(request.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Profile with ID {request.UserProfileId} not found.");

            var today = userProfile.LocalDateOn(clock.GetCurrentInstant().ToDateTimeUtc());

            logger.LogDebug("Resolving active quests for user {UserProfileId} on local date {Today}.",
                request.UserProfileId, today);

            var quests = await unitOfWork.Quests
                .GetActiveQuestsForDisplayAsync(request.UserProfileId, today, cancellationToken)
                .ConfigureAwait(false);

            // The date-range filter happens in SQL; whether a quest is actually due today is decided by the
            // one schedule implementation, in memory. A user has tens of quests, not thousands.
            return quests
                .Where(quest => quest.IsDueOn(today))
                .Select(quest => questMapper.MapToDto(quest, today))
                .ToList();
        }
    }
}
