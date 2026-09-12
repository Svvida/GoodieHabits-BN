using Application.Badges;
using Application.Common;
using Application.Quests.Dtos;
using Application.Quests.Utilities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using MediatR;
using NodaTime;

namespace Application.Quests.Commands.CreateQuest
{
    public class CreateQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMapper,
        IBadgeAwardingService badgeAwardingService,
        IClock clock)
        : IRequestHandler<CreateQuestCommand, QuestDetailsDto>
    {
        public async Task<QuestDetailsDto> Handle(CreateQuestCommand command, CancellationToken cancellationToken)
        {
            var userProfile = await unitOfWork.UserProfiles.GetUserProfileWithBadgesAsync(command.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"User Profile with ID: {command.UserProfileId} not found.");

            var nowUtc = clock.GetCurrentInstant().ToDateTimeUtc();
            var today = userProfile.LocalDateOn(nowUtc);

            var quest = Quest.Create(
                title: command.Title,
                userProfile: userProfile,
                schedule: QuestScheduleTranslator.ToSchedule(command.Schedule),
                target: QuestScheduleTranslator.ToTarget(command.Target),
                nowUtc: nowUtc,
                description: command.Description,
                priority: EnumHelper.ParseNullable<PriorityEnum>(command.Priority),
                emoji: command.Emoji,
                startDate: command.StartDate,
                endDate: command.EndDate,
                difficulty: EnumHelper.ParseNullable<DifficultyEnum>(command.Difficulty),
                scheduledTime: command.ScheduledTime,
                durationMinutes: command.DurationMinutes,
                labelIds: command.Labels);

            await unitOfWork.Quests.AddAsync(quest, cancellationToken).ConfigureAwait(false);

            // Materialize from the quest's start (or today) so the first period exists before the user can
            // reach for it — there is no reset job to do this later.
            quest.InitializePeriods(today);
            quest.RecalculateStatistics(today);

            userProfile.UpdateAfterQuestCreation();

            await badgeAwardingService.CheckAndAwardBadgesAsync(BadgeTriggerEnum.QuestCreated, userProfile, null, cancellationToken).ConfigureAwait(false);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return questMapper.MapToDto(quest, today);
        }
    }
}
