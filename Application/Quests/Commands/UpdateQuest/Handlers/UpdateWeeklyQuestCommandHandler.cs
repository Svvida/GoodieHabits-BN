using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;
using NodaTime;

namespace Application.Quests.Commands.UpdateQuest.Handlers
{
    public class UpdateWeeklyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService)
        : UpdateQuestCommandHandler<UpdateWeeklyQuestCommand, WeeklyQuestDetailsDto>(
            unitOfWork,
            questMappingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, UpdateWeeklyQuestCommand command, CancellationToken cancellationToken)
        {
            var weekdays = command.Weekdays.Select(d => Enum.Parse<WeekdayEnum>(d, true)).ToHashSet();
            quest.SetWeekdays(weekdays);

            var today = quest.UserProfile.LocalDateOn(SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc());

            // If today's period is no longer part of the schedule, drop it — it was never really due.
            // A period the user already completed is left alone so history and streaks stay intact.
            var todaysOccurrence = quest.QuestOccurrences.FirstOrDefault(o => o.Covers(today));
            if (todaysOccurrence is not null
                && !todaysOccurrence.WasCompleted
                && !weekdays.Contains((WeekdayEnum)todaysOccurrence.PeriodStart.DayOfWeek))
            {
                quest.QuestOccurrences.Remove(todaysOccurrence);
            }

            return Task.CompletedTask;
        }
    }
}
