using Application.Quests.Dtos;
using Domain.Interfaces;

namespace Application.Quests.UpdateQuest.Handlers
{
    public class UpdateOneTimeQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestResetService questResetService,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMapper questMappingService)
        : UpdateQuestCommandHandler<UpdateOneTimeQuestCommand, OneTimeQuestDetailsDto>(
            unitOfWork,
            questResetService,
            questOccurrenceGenerator,
            questMappingService)
    {
    }
}
