using Application.Commands;
using Application.Common;
using Application.Interfaces.Quests;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.CommandsHandlers
{
    public class UpdateQuestCompletionCommandHandler : IRequestHandler<UpdateQuestCompletionCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPublisher _publisher;
        private readonly IQuestResetService _questResetService;
        private readonly ILogger<UpdateQuestCompletionCommandHandler> _logger;
        private readonly IQuestOccurrenceGenerator _questOccurrenceGenerator;
        private readonly IQuestStatisticsService _questStatisticsService;

        public UpdateQuestCompletionCommandHandler(
            IUnitOfWork unitOfWork,
            IPublisher publisher,
            IQuestResetService questResetService,
            ILogger<UpdateQuestCompletionCommandHandler> logger,
            IQuestOccurrenceGenerator questOccurrenceGenerator,
            IQuestStatisticsService questStatisticsService)
        {
            _unitOfWork = unitOfWork;
            _publisher = publisher;
            _questResetService = questResetService;
            _logger = logger;
            _questOccurrenceGenerator = questOccurrenceGenerator;
            _questStatisticsService = questStatisticsService;
        }

        public async Task<Unit> Handle(UpdateQuestCompletionCommand command, CancellationToken cancellationToken = default)
        {
            var quest = await GetAndValidateQuestAsync(command.QuestId, command.QuestType, cancellationToken).ConfigureAwait(false);

            if (quest.IsCompleted == command.IsCompleted)
            {
                _logger.LogDebug("Quest {QuestId} completion status is unchanged.", quest.Id);
                return Unit.Value;
            }
            _logger.LogDebug("I'm in handler.");
            if (!command.IsCompleted && quest.UserGoal?.Count > 0)
                throw new ConflictException("Cannot uncomplete a quest that is an active goal");

            var nowUtc = SystemClock.Instance.GetCurrentInstant();

            if (quest.IsRepeatable())
            {
                var currentOccurrence = await GetOrCreateQuestOccurrenceAsync(quest, nowUtc.ToDateTimeUtc(), cancellationToken).ConfigureAwait(false);
                if (currentOccurrence is not null)
                {
                    quest.AddOccurence(currentOccurrence);
                }
                else
                {
                    throw new ConflictException("Could not find an active or recent quest period to apply this completion to.");
                }
            }

            if (command.IsCompleted)
            {
                quest.Complete(nowUtc.ToDateTimeUtc(), _questResetService, ShouldAssignRewards(quest, nowUtc));
            }
            else
            {
                quest.Uncomplete();
            }

            foreach (var domainEvent in quest.DomainEvents)
            {
                var notification = CreateDomainEventNotification(domainEvent);
                await _publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
            }
            quest.ClearDomainEvents();

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogDebug("Quest {QuestId} completion processed", quest.Id);

            if (quest.IsRepeatable())
                await _questStatisticsService.ProcessStatisticsForQuestAndSaveAsync(quest.Id, cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }

        private async Task<Quest> GetAndValidateQuestAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken)
        {
            var quest = await _unitOfWork.Quests.GetQuestByIdAsync(questId, questType, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {questId} not found");

            if (quest.Account is null || string.IsNullOrWhiteSpace(quest.Account.TimeZone))
            {
                _logger.LogError("Account {AccountId} data or TimeZone is missing for Quest {QuestId}. Cannot accurately perform daily completion check.",
                    quest.AccountId, quest.Id);
                throw new InvalidArgumentException($"TimeZone information is missing for the account associated with Quest {quest.Id}.");
            }

            // Quest can be assigned to only one active goal at a time
            var goal = await _unitOfWork.UserGoals.GetActiveGoalByQuestIdAsync(questId, cancellationToken).ConfigureAwait(false);
            if (goal is not null)
                quest.UserGoal.Add(goal);

            return quest;
        }

        // Check if quest was already completed today
        private bool ShouldAssignRewards(Quest quest, Instant nowUtc)
        {
            if (!quest.LastCompletedAt.HasValue)
                return true;

            var userTimeZone = DateTimeZoneProviders.Tzdb[quest.Account.TimeZone]
                ?? throw new NotFoundException($"Timezone with ID: {quest.Account.TimeZone} not found");

            var lastCompletedAtUtc = Instant.FromDateTimeUtc(DateTime.SpecifyKind(quest.LastCompletedAt.Value, DateTimeKind.Utc));
            var lastCompletedAtUserLocal = lastCompletedAtUtc.InZone(userTimeZone).LocalDateTime;
            var nowUserLocal = nowUtc.InZone(userTimeZone).LocalDateTime;

            if (lastCompletedAtUserLocal.Date < nowUserLocal.Date)
                return true;

            _logger.LogInformation("Quest {QuestId} already completed today: {CurrentTime} in user's timezone {TimeZone}. Last Completion: {LastCompletion}",
                quest.Id, nowUserLocal, quest.Account.TimeZone, lastCompletedAtUserLocal);

            return false;
        }

        private async Task<QuestOccurrence?> GetOrCreateQuestOccurrenceAsync(Quest quest, DateTime nowUtc, CancellationToken cancellationToken)
        {
            var currentOccurrence = await _unitOfWork.QuestOccurrences.GetCurrentOccurrenceForQuestAsync(quest.Id, nowUtc, cancellationToken).ConfigureAwait(false);
            if (currentOccurrence is null)
            {
                var missingOccurences = await _questOccurrenceGenerator.GenerateMissingOccurrencesForQuestAsync(quest, cancellationToken).ConfigureAwait(false);
                currentOccurrence = missingOccurences.FirstOrDefault(o => o.OccurrenceStart <= nowUtc && o.OccurrenceEnd >= nowUtc);
            }
            if (currentOccurrence is null)
            {
                currentOccurrence = await _unitOfWork.QuestOccurrences.GetLastOccurrenceForCompletionAsync(quest.Id, nowUtc, cancellationToken).ConfigureAwait(false);
                if (currentOccurrence is null)
                    return null;

                if (nowUtc > currentOccurrence.OccurrenceEnd.AddHours(24))
                    return null;
            }
            return currentOccurrence;
        }

        private static INotification CreateDomainEventNotification(object domainEvent)
        {
            var genericType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            return (INotification)Activator.CreateInstance(genericType, domainEvent)!;
        }
    }
}
