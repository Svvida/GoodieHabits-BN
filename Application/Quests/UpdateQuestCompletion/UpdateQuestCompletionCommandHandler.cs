using Application.Common;
using Application.Common.Interfaces.Quests;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Quests.UpdateQuestCompletion
{
    public class UpdateQuestCompletionCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestResetService questResetService,
        ILogger<UpdateQuestCompletionCommandHandler> logger,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestStatisticsService questStatisticsService) : IRequestHandler<UpdateQuestCompletionCommand, Unit>
    {
        public async Task<Unit> Handle(UpdateQuestCompletionCommand command, CancellationToken cancellationToken = default)
        {
            var quest = await GetAndValidateQuestAsync(command.QuestId, command.QuestType, cancellationToken).ConfigureAwait(false);

            if (quest.IsCompleted == command.IsCompleted)
            {
                logger.LogDebug("Quest {QuestId} completion status is unchanged.", quest.Id);
                return Unit.Value;
            }

            if (!command.IsCompleted && quest.UserGoal?.Count > 0)
                throw new ConflictException("Cannot uncomplete a quest that is an active goal");

            var nowUtc = SystemClock.Instance.GetCurrentInstant();

            if (quest.IsRepeatable())
            {
                var currentOccurrence = await GetOrCreateQuestOccurrenceAsync(quest, nowUtc.ToDateTimeUtc(), cancellationToken).ConfigureAwait(false);
                if (currentOccurrence is not null)
                {
                    quest.AddOccurrence(currentOccurrence);
                }
                else
                {
                    throw new ConflictException("Could not find an active or recent quest period to apply this completion to.");
                }
            }

            if (command.IsCompleted)
            {
                quest.Complete(nowUtc.ToDateTimeUtc(), questResetService, ShouldAssignRewards(quest, nowUtc));
            }
            else
            {
                quest.Uncomplete();
            }

            foreach (var domainEvent in quest.DomainEvents)
            {
                var notification = DomainEventsHelper.CreateDomainEventNotification(domainEvent);
                await publisher.Publish(notification, cancellationToken).ConfigureAwait(false);
            }
            quest.ClearDomainEvents();

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogDebug("Quest {QuestId} completion processed", quest.Id);

            if (quest.IsRepeatable())
                await questStatisticsService.ProcessStatisticsForQuestAndSaveAsync(quest.Id, cancellationToken).ConfigureAwait(false);

            return Unit.Value;
        }

        private async Task<Quest> GetAndValidateQuestAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken)
        {
            var quest = await unitOfWork.Quests.GetQuestByIdAsync(questId, questType, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {questId} not found");

            if (quest.Account is null || string.IsNullOrWhiteSpace(quest.Account.TimeZone))
            {
                logger.LogError("Account {AccountId} data or TimeZone is missing for Quest {QuestId}. Cannot accurately perform daily completion check.",
                    quest.AccountId, quest.Id);
                throw new InvalidArgumentException($"TimeZone information is missing for the account associated with Quest {quest.Id}.");
            }

            // Quest can be assigned to only one active goal at a time
            // Since quest is tracked we can just execute this to assign it to the goal if it exists
            await unitOfWork.UserGoals.GetActiveGoalByQuestIdAsync(questId, cancellationToken).ConfigureAwait(false);

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

            logger.LogInformation("Quest {QuestId} already completed today: {CurrentTime} in user's timezone {TimeZone}. Last Completion: {LastCompletion}",
                quest.Id, nowUserLocal, quest.Account.TimeZone, lastCompletedAtUserLocal);

            return false;
        }

        private async Task<QuestOccurrence?> GetOrCreateQuestOccurrenceAsync(Quest quest, DateTime nowUtc, CancellationToken cancellationToken)
        {
            var currentOccurrence = await unitOfWork.QuestOccurrences.GetCurrentOccurrenceForQuestAsync(quest.Id, nowUtc, cancellationToken).ConfigureAwait(false);
            logger.LogDebug("Current occurrence for quest {QuestId} at {NowUtc}: {Start} {End}", quest.Id, nowUtc, currentOccurrence?.OccurrenceStart, currentOccurrence?.OccurrenceEnd);
            if (currentOccurrence is null)
            {
                var missingOccurences = await questOccurrenceGenerator.GenerateOccurrenceForNewQuest(quest, cancellationToken).ConfigureAwait(false);
                currentOccurrence = missingOccurences.FirstOrDefault(o => o.OccurrenceStart <= nowUtc && o.OccurrenceEnd >= nowUtc);
                logger.LogDebug("Current occurrence for quest {QuestId} at {NowUtc}: {Start} {End}", quest.Id, nowUtc, currentOccurrence?.OccurrenceStart, currentOccurrence?.OccurrenceEnd);
            }
            if (currentOccurrence is null)
            {
                currentOccurrence = await unitOfWork.QuestOccurrences.GetLastOccurrenceForCompletionAsync(quest.Id, nowUtc, cancellationToken).ConfigureAwait(false);
                logger.LogDebug("Current occurrence for quest {QuestId} at {NowUtc}: {Start} {End}", quest.Id, nowUtc, currentOccurrence?.OccurrenceStart, currentOccurrence?.OccurrenceEnd);
                if (currentOccurrence is null)
                    return null;

                if (nowUtc > currentOccurrence.OccurrenceEnd.AddHours(24))
                    return null;
            }
            return currentOccurrence;
        }
    }
}
