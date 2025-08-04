using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Quests.UpdateQuest.Handlers
{
    public class UpdateMonthlyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestResetService questResetService,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMappingService questMappingService)
        : UpdateQuestCommandHandler<UpdateMonthlyQuestCommand, MonthlyQuestDetailsDto>(
            unitOfWork,
            questResetService,
            questOccurrenceGenerator,
            questMappingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, UpdateMonthlyQuestCommand command, CancellationToken cancellationToken)
        {
            quest.SetMonthlyDays(command.StartDay, command.EndDay);
            return Task.CompletedTask;
        }
    }
}
