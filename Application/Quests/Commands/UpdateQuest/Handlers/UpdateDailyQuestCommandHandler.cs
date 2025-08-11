using Application.Quests.Commands.UpdateQuest;
using Application.Quests.Dtos;
using Domain.Interfaces;

namespace Application.Quests.Commands.UpdateQuest.Handlers
{
    public class UpdateDailyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService)
        : UpdateQuestCommandHandler<UpdateDailyQuestCommand, DailyQuestDetailsDto>(
            unitOfWork,
            questMappingService)
    {
    }
}
