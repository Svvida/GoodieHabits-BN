using Application.Badges;
using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Quests.Commands.CreateQuest.Handlers
{
    public class CreateWeeklyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService,
        IBadgeAwardingService badgeAwardingService)
        : CreateQuestCommandHandler<CreateWeeklyQuestCommand, WeeklyQuestDetailsDto>(
            unitOfWork,
            questMappingService,
            badgeAwardingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, CreateWeeklyQuestCommand command, CancellationToken cancellationToken)
        {
            var weekdays = command.Weekdays.Select(d => Enum.Parse<WeekdayEnum>(d, true));
            quest.SetWeekdays(weekdays);
            return Task.CompletedTask;
        }
    }
}
