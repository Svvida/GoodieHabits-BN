using Application.Quests.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.CreateQuest.Handlers
{
    public class CreateOneTimeQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestMapper questMappingService)
        : CreateQuestCommandHandler<CreateOneTimeQuestCommand, OneTimeQuestDetailsDto>(
            unitOfWork,
            publisher,
            questMappingService)
    {
    }
}
