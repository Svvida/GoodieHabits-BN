using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Interfaces;
using Domain.Models;
using MediatR;

namespace Application.Quests.CreateQuest.Handlers
{
    internal class CreateMonthlyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMappingService questMappingService,
        IQuestResetService questResetService)
        : CreateQuestCommandHandler<CreateMonthlyQuestCommand, MonthlyQuestDetailsDto>(
            unitOfWork,
            publisher,
            questOccurrenceGenerator,
            questMappingService,
            questResetService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, CreateMonthlyQuestCommand command, CancellationToken cancellationToken)
        {
            quest.SetMonthlyDays(command.StartDay, command.EndDay);
            return Task.CompletedTask;
        }
    }
}
