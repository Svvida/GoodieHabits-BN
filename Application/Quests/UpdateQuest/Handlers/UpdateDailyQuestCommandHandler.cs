using Application.Quests.Dtos;
using Domain.Interfaces;

namespace Application.Quests.UpdateQuest.Handlers
{
    public class UpdateDailyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestResetService questResetService,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMapper questMappingService)
        : UpdateQuestCommandHandler<UpdateDailyQuestCommand, DailyQuestDetailsDto>(
            unitOfWork,
            questResetService,
            questOccurrenceGenerator,
            questMappingService)
    {
    }
}
