using Application.Quests.Commands.UpdateQuest;
using Application.Quests.Dtos;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Models;

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
            return Task.CompletedTask;
        }
    }
}
