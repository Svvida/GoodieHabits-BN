using Application.Quests.Commands.CreateQuest;
using Application.Quests.Dtos;
using Domain.Interfaces;
using Domain.Models;
using MediatR;

namespace Application.Quests.Commands.CreateQuest.Handlers
{
    internal class CreateMonthlyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestMapper questMappingService)
        : CreateQuestCommandHandler<CreateMonthlyQuestCommand, MonthlyQuestDetailsDto>(
            unitOfWork,
            publisher,
            questMappingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, CreateMonthlyQuestCommand command, CancellationToken cancellationToken)
        {
            quest.SetMonthlyDays(command.StartDay, command.EndDay);
            return Task.CompletedTask;
        }
    }
}
