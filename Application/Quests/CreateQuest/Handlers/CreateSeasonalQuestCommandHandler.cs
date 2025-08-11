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
        IQuestMapper questMappingService)
        : CreateQuestCommandHandler<CreateSeasonalQuestCommand, SeasonalQuestDetailsDto>(
            unitOfWork,
            publisher,
            questMappingService)
    {
        protected override Task HandleQuestSpecificsAsync(Quest quest, CreateSeasonalQuestCommand command, CancellationToken cancellationToken)
        {
            quest.SetSeason(Enum.Parse<SeasonEnum>(command.Season, true));
            return Task.CompletedTask;
        }
    }
}
