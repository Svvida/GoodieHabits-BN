using Application.Common;
using Application.Quests.Dtos;
using Application.Quests.Utilities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Quests.Commands.UpdateQuest
{
    public class UpdateQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMapper,
        IClock clock)
        : IRequestHandler<UpdateQuestCommand, QuestDetailsDto>
    {
        public async Task<QuestDetailsDto> Handle(UpdateQuestCommand command, CancellationToken cancellationToken)
        {
            var quest = await unitOfWork.Quests.GetQuestByIdForUpdateAsync(command.QuestId, command.UserProfileId, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID {command.QuestId} not found.");

            var nowUtc = clock.GetCurrentInstant().ToDateTimeUtc();
            var today = quest.UserProfile.LocalDateOn(nowUtc);

            quest.UpdateTitle(command.Title);
            quest.UpdateDescription(command.Description);
            quest.UpdatePriority(EnumHelper.ParseNullable<PriorityEnum>(command.Priority));
            quest.UpdateEmoji(command.Emoji);
            quest.UpdateScheduledTime(command.ScheduledTime);
            quest.UpdateDuration(command.DurationMinutes);
            quest.UpdateDifficulty(EnumHelper.ParseNullable<DifficultyEnum>(command.Difficulty));
            quest.UpdateDates(command.StartDate, command.EndDate);
            quest.SetLabels(command.Labels);

            // Order matters: the schedule decides which periods exist, the target decides what each asks for.
            quest.UpdateSchedule(QuestScheduleTranslator.ToSchedule(command.Schedule), nowUtc, today);
            quest.UpdateTarget(QuestScheduleTranslator.ToTarget(command.Target), nowUtc, today);

            // The date range may have moved, which can open periods that did not exist before.
            quest.GenerateMissingPeriodsOn(today);
            quest.RecalculateStatistics(today);

            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return questMapper.MapToDto(quest, today);
        }
    }
}
