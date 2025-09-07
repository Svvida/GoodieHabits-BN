using Application.Badges;
using Application.Quests.Dtos;
using Domain.Interfaces;

namespace Application.Quests.Commands.CreateQuest.Handlers
{
    public class CreateDailyQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService,
        IBadgeAwardingService badgeAwardingService)
        : CreateQuestCommandHandler<CreateDailyQuestCommand, DailyQuestDetailsDto>(
            unitOfWork,
            questMappingService,
            badgeAwardingService)
    {
    }
}
