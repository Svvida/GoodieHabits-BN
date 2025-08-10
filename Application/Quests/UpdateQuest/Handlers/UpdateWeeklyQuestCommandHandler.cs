using Application.Quests.Dtos;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Quests.UpdateQuest.Handlers
{
    public class UpdateWeeklyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestResetService questResetService,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMapper questMappingService)
        : UpdateQuestCommandHandler<UpdateWeeklyQuestCommand, WeeklyQuestDetailsDto>(
            unitOfWork,
            questResetService,
            questOccurrenceGenerator,
            questMappingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, UpdateWeeklyQuestCommand command, CancellationToken cancellationToken)
        {
            var weekdays = command.Weekdays.Select(d => Enum.Parse<WeekdayEnum>(d, true));
            quest.SetWeekdays(weekdays);
            return Task.CompletedTask;
        }
    }
}
