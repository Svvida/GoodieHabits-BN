using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Interfaces;
using MediatR;

namespace Application.Quests.CreateQuest.Handlers
{
    public class CreateOneTimeQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMappingService questMappingService,
        IQuestResetService questResetService)
        : CreateQuestCommandHandler<CreateOneTimeQuestCommand, OneTimeQuestDetailsDto>(
            unitOfWork,
            publisher,
            questOccurrenceGenerator,
            questMappingService,
            questResetService)
    {
    }
}
