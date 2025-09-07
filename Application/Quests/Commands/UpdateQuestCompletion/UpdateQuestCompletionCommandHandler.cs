using Application.Badges;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Quests.Commands.UpdateQuestCompletion
{
    public class UpdateQuestCompletionCommandHandler(
        IUnitOfWork unitOfWork,
        IBadgeAwardingService badgeAwardingService,
        ILogger<UpdateQuestCompletionCommandHandler> logger) : IRequestHandler<UpdateQuestCompletionCommand, Unit>
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

            if (command.IsCompleted)
            {
                quest.Complete(nowUtc.ToDateTimeUtc(), ShouldAssignRewards(quest, nowUtc));
                await badgeAwardingService.CheckAndAwardBadgesAsync(BadgeTriggerEnum.QuestCompleted, quest.UserProfile, quest, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                quest.Uncomplete(nowUtc.ToDateTimeUtc());
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            logger.LogDebug("Quest {QuestId} completion processed", quest.Id);

            return Unit.Value;
        }

        private async Task<Quest> GetAndValidateQuestAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken)
        {
            var quest = await unitOfWork.Quests.GetQuestByIdForCompletionUpdateAsync(questId, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {questId} not found");

            if (quest.UserProfile is null || string.IsNullOrWhiteSpace(quest.UserProfile.TimeZone))
            {
                logger.LogError("User Profile {UserProfileId} data or TimeZone is missing for Quest {QuestId}. Cannot accurately perform daily completion check.",
                    quest.UserProfileId, quest.Id);
                throw new InvalidArgumentException($"TimeZone information is missing for the account associated with Quest {quest.Id}.");
            }

            // We can just call this, EF tracker will connect goal to the quest if it is returned
            await unitOfWork.UserGoals.GetActiveGoalByQuestIdAsync(questId, cancellationToken).ConfigureAwait(false);

            return quest;
        }

        // Check if quest was already completed today
        private bool ShouldAssignRewards(Quest quest, Instant nowUtc)
        {
            if (!quest.LastCompletedAt.HasValue)
                return true;

            var userTimeZone = DateTimeZoneProviders.Tzdb[quest.UserProfile.TimeZone]
                ?? throw new NotFoundException($"Timezone with ID: {quest.UserProfile.TimeZone} not found");

            var lastCompletedAtUtc = Instant.FromDateTimeUtc(DateTime.SpecifyKind(quest.LastCompletedAt.Value, DateTimeKind.Utc));
            var lastCompletedAtUserLocal = lastCompletedAtUtc.InZone(userTimeZone).LocalDateTime;
            var nowUserLocal = nowUtc.InZone(userTimeZone).LocalDateTime;

            if (lastCompletedAtUserLocal.Date < nowUserLocal.Date)
                return true;

            logger.LogInformation("Quest {QuestId} already completed today: {CurrentTime} in user's timezone {TimeZone}. Last Completion: {LastCompletion}",
                quest.Id, nowUserLocal, quest.UserProfile.TimeZone, lastCompletedAtUserLocal);

            return false;
        }
    }
}
