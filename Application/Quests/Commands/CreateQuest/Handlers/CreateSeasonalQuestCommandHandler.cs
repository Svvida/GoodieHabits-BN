using Application.Badges;
using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Quests.Commands.CreateQuest.Handlers
{
    public class CreateSeasonalQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService,
        IBadgeAwardingService badgeAwardingService)
        : CreateQuestCommandHandler<CreateSeasonalQuestCommand, SeasonalQuestDetailsDto>(
            unitOfWork,
            questMappingService,
            badgeAwardingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, CreateSeasonalQuestCommand command, CancellationToken cancellationToken)
        {
            quest.SetSeason(Enum.Parse<SeasonEnum>(command.Season, true));
            return Task.CompletedTask;
        }
    }
}
