using Application.Quests.Commands.UpdateQuest;
using Application.Quests.Dtos;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Quests.Commands.UpdateQuest.Handlers
{
    public class UpdateMonthlyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService)
        : UpdateQuestCommandHandler<UpdateMonthlyQuestCommand, MonthlyQuestDetailsDto>(
            unitOfWork,
            questMappingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, UpdateMonthlyQuestCommand command, CancellationToken cancellationToken)
        {
            quest.SetMonthlyDays(command.StartDay, command.EndDay);
            return Task.CompletedTask;
        }
    }
}
