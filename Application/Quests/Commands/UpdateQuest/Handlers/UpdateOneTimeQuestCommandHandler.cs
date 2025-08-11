using Application.Quests.Commands.UpdateQuest;
using Application.Quests.Dtos;
using Domain.Interfaces;

namespace Application.Quests.Commands.UpdateQuest.Handlers
{
    public class UpdateOneTimeQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService)
        : UpdateQuestCommandHandler<UpdateOneTimeQuestCommand, OneTimeQuestDetailsDto>(
            unitOfWork,
            questMappingService)
    {
    }
}
