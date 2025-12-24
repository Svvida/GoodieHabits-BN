using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using NodaTime;
using NodaTime.Extensions;

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
            var weekdays = command.Weekdays.Select(d => Enum.Parse<WeekdayEnum>(d, true));
            quest.SetWeekdays(weekdays);

            var lastOccurrence = quest.QuestOccurrences
                .OrderByDescending(o => o.OccurrenceEnd)
                .FirstOrDefault();
            if (lastOccurrence != null)
            {
                var nowUtc = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
                if (lastOccurrence.OccurrenceStart < nowUtc && lastOccurrence.OccurrenceEnd > nowUtc)
                {
                    var userTimeZone = DateTimeZoneProviders.Tzdb[quest.UserProfile.TimeZone]
                        ?? throw new NotFoundException($"Timezone with ID: {quest.UserProfile.TimeZone} not found");

                    var instant = Instant.FromDateTimeUtc(DateTime.SpecifyKind(lastOccurrence.OccurrenceStart, DateTimeKind.Utc));
                    var zonedDateTime = instant.InZone(userTimeZone);
                    var occurrenceLocalStartWeekday = zonedDateTime.DayOfWeek.ToDayOfWeek();
                    if (!weekdays.Any(w => w == (WeekdayEnum)occurrenceLocalStartWeekday))
                    {
                        quest.QuestOccurrences.Remove(lastOccurrence);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
