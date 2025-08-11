using Application.Quests.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.CreateQuest.Handlers
{
    public class CreateDailyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestMapper questMappingService)
        : CreateQuestCommandHandler<CreateDailyQuestCommand, DailyQuestDetailsDto>(
            unitOfWork,
            publisher,
            questMappingService)
    {
    }
}
