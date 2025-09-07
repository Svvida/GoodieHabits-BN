using Application.Badges;
using Application.Quests.Dtos;
using Domain.Interfaces;

namespace Application.Quests.Commands.CreateQuest.Handlers
{
    public class CreateOneTimeQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService,
        IBadgeAwardingService badgeAwardingService)
        : CreateQuestCommandHandler<CreateOneTimeQuestCommand, OneTimeQuestDetailsDto>(
            unitOfWork,
            questMappingService,
            badgeAwardingService)
    {
    }
}
