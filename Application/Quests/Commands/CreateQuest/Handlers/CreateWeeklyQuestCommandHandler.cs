using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;
using MediatR;

namespace Application.Quests.Commands.CreateQuest.Handlers
{
    public class CreateWeeklyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestMapper questMappingService)
        : CreateQuestCommandHandler<CreateWeeklyQuestCommand, WeeklyQuestDetailsDto>(
            unitOfWork,
            publisher,
            questMappingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, CreateWeeklyQuestCommand command, CancellationToken cancellationToken)
        {
            var weekdays = command.Weekdays.Select(d => Enum.Parse<WeekdayEnum>(d, true));
            quest.SetWeekdays(weekdays);
            return Task.CompletedTask;
        }
    }
}
