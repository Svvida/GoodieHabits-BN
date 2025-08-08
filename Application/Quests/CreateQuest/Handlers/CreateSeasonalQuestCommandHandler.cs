using Application.Common.Interfaces.Quests;
using Application.Quests.Dtos;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Models;
using MediatR;

namespace Application.Quests.CreateQuest.Handlers
{
    public class CreateSeasonalQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMapper questMappingService,
        IQuestResetService questResetService)
        : CreateQuestCommandHandler<CreateSeasonalQuestCommand, SeasonalQuestDetailsDto>(
            unitOfWork,
            publisher,
            questOccurrenceGenerator,
            questMappingService,
            questResetService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, CreateSeasonalQuestCommand command, CancellationToken cancellationToken)
        {
            quest.SetSeason(Enum.Parse<SeasonEnum>(command.Season, true));
            return Task.CompletedTask;
        }
    }
}
