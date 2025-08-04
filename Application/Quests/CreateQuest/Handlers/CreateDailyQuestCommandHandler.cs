using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.CreateQuest.Handlers
{
    public class CreateDailyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMappingService questMappingService,
        IQuestResetService questResetService)
        : CreateQuestCommandHandler<CreateDailyQuestCommand, DailyQuestDetailsDto>(
            unitOfWork,
            publisher,
            questOccurrenceGenerator,
            questMappingService,
            questResetService)
    {
    }
}
