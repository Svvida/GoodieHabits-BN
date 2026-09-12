using Application.Badges;
using Application.Common.Notifications;
using Application.Quests.Utilities;
using Application.Statistics.Calculators;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Quests.Commands.AddQuestCompletion
{
    public class AddQuestCompletionCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMapper,
        IBadgeAwardingService badgeAwardingService,
        ILevelCalculator levelCalculator,
        IPublisher publisher,
        ILogger<AddQuestCompletionCommandHandler> logger,
        IClock clock)
        : IRequestHandler<AddQuestCompletionCommand, QuestCompletionResponse>
    {
        public async Task<QuestCompletionResponse> Handle(AddQuestCompletionCommand command, CancellationToken cancellationToken)
        {
            var quest = await unitOfWork.Quests
                .GetQuestForCompletionAsync(command.QuestId, command.UserProfileId, cancellationToken)
                .ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {command.QuestId} not found");

            if (quest.UserProfile is null || string.IsNullOrWhiteSpace(quest.UserProfile.TimeZone))
            {
                logger.LogError("User Profile {UserProfileId} data or TimeZone is missing for Quest {QuestId}.",
                    quest.UserProfileId, quest.Id);
                throw new InvalidArgumentException($"TimeZone information is missing for the account associated with Quest {quest.Id}.");
            }

            // EF's change tracker attaches the goal to the quest, so the entity can achieve it.
            await unitOfWork.UserGoals.GetActiveGoalByQuestIdAsync(command.QuestId, cancellationToken).ConfigureAwait(false);

            var nowUtc = clock.GetCurrentInstant().ToDateTimeUtc();
            var today = quest.UserProfile.LocalDateOn(nowUtc);

            int levelBefore = levelCalculator.CalculateLevelInfo(quest.UserProfile.TotalXp).CurrentLevel;

            var result = quest.AddCompletion(
                nowUtc: nowUtc,
                today: today,
                amount: command.Amount,
                completedOn: command.CompletedOn,
                localTime: LocalTimeResolver.Resolve(quest.UserProfile.TimeZone, nowUtc),
                source: CompletionSourceEnum.App,
                clientRequestId: command.ClientRequestId,
                note: command.Note);

            if (result.PeriodCompleted)
            {
                await badgeAwardingService
                    .CheckAndAwardBadgesAsync(BadgeTriggerEnum.QuestCompleted, quest.UserProfile, quest, cancellationToken)
                    .ConfigureAwait(false);

                await NotifyIfLeveledUpAsync(quest, levelBefore, cancellationToken).ConfigureAwait(false);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return BuildResponse(quest, today, result);
        }

        private async Task NotifyIfLeveledUpAsync(Quest quest, int levelBefore, CancellationToken cancellationToken)
        {
            int levelAfter = levelCalculator.CalculateLevelInfo(quest.UserProfile.TotalXp).CurrentLevel;

            if (levelAfter <= levelBefore)
                return;

            logger.LogDebug("UserProfile {UserProfileId} leveled up from {OldLevel} to {NewLevel} after completing Quest {QuestId}.",
                quest.UserProfileId, levelBefore, levelAfter, quest.Id);

            await publisher.Publish(new UserLeveledUpNotification(quest.UserProfileId, levelAfter), cancellationToken)
                .ConfigureAwait(false);
        }

        private QuestCompletionResponse BuildResponse(Quest quest, DateOnly today, Domain.ValueObjects.QuestCompletionResult result)
        {
            var completion = result.Completion;

            return new QuestCompletionResponse(
                Quest: questMapper.MapToDto(quest, today),
                Completion: new QuestCompletionDto(
                    completion.Id,
                    completion.QuestId,
                    completion.OccurrenceId,
                    completion.CompletedOn,
                    completion.CompletedAt,
                    completion.LocalTime,
                    completion.Amount,
                    completion.IsBackfilled,
                    completion.IsOffSchedule,
                    completion.Source.ToString(),
                    completion.Note),
                WasAlreadyRecorded: result.WasAlreadyRecorded,
                PeriodCompleted: result.PeriodCompleted,
                XpAwarded: result.Reward.Xp,
                CoinsAwarded: result.Reward.Coins);
        }
    }
}
