using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Quests.UpdateQuest.Handlers
{
    public class UpdateSeasonalQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestResetService questResetService,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMapper questMappingService)
        : UpdateQuestCommandHandler<UpdateSeasonalQuestCommand, SeasonalQuestDetailsDto>(
            unitOfWork,
            questResetService,
            questOccurrenceGenerator,
            questMappingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, UpdateSeasonalQuestCommand command, CancellationToken cancellationToken)
        {
            quest.SetSeason(Enum.Parse<SeasonEnum>(command.Season, true));
            return Task.CompletedTask;
        }
    }
}
