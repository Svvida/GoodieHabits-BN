using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Interfaces;

namespace Application.Quests.UpdateQuest.Handlers
{
    public class UpdateDailyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestResetService questResetService,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMappingService questMappingService)
        : UpdateQuestCommandHandler<UpdateDailyQuestCommand, DailyQuestDetailsDto>(
            unitOfWork,
            questResetService,
            questOccurrenceGenerator,
            questMappingService)
    {
    }
}
