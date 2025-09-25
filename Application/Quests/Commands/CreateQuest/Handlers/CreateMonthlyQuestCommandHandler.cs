using Application.Badges;
using Application.Quests.Dtos;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Quests.Commands.CreateQuest.Handlers
{
    internal class CreateMonthlyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService,
        IBadgeAwardingService badgeAwardingService)
        : CreateQuestCommandHandler<CreateMonthlyQuestCommand, MonthlyQuestDetailsDto>(
            unitOfWork,
            questMappingService,
            badgeAwardingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, CreateMonthlyQuestCommand command, CancellationToken cancellationToken)
        {
            quest.SetMonthlyDays(command.StartDay, command.EndDay);
            return Task.CompletedTask;
        }
    }
}
