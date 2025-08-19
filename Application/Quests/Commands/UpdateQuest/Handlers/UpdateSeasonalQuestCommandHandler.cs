using Application.Quests.Commands.UpdateQuest;
using Application.Quests.Dtos;
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Quests.Commands.UpdateQuest.Handlers
{
    public class UpdateSeasonalQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestMapper questMappingService)
        : UpdateQuestCommandHandler<UpdateSeasonalQuestCommand, SeasonalQuestDetailsDto>(
            unitOfWork,
            questMappingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, UpdateSeasonalQuestCommand command, CancellationToken cancellationToken)
        {
            quest.SetSeason(Enum.Parse<SeasonEnum>(command.Season, true));
            return Task.CompletedTask;
        }
    }
}
