using Application.Dtos.Quests;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces.Quests;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using MediatR;
using NodaTime;

namespace Application.Quests.Commands.UpdateQuest
{
    public class UpdateQuestCommandHandler(
        IUnitOfWork unitOfWork,
        IQuestResetService questResetService,
        IQuestOccurrenceGenerator questOccurrenceGenerator,
        IQuestMappingService questMappingService) : IRequestHandler<UpdateQuestCommand, BaseGetQuestDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IQuestOccurrenceGenerator _questOccurrenceGenerator = questOccurrenceGenerator;
        private readonly IQuestMappingService _questMappingService = questMappingService;

        public async Task<BaseGetQuestDto> Handle(UpdateQuestCommand request, CancellationToken cancellationToken)
        {
            var updateDto = request.UpdateDto;

            var quest = await _unitOfWork.Quests.GetQuestByIdAsync(updateDto.Id, updateDto.QuestType, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID {updateDto.Id} not found.");

            quest.UpdateDescription(updateDto.Description);

            quest.UpdatePriority(Enum.TryParse<PriorityEnum>(updateDto.Priority, true, out var priority) ? priority : null);

            quest.UpdateEmoji(updateDto.Emoji);

            quest.UpdateScheduledTime(updateDto.ScheduledTime);

            quest.UpdateDifficulty(Enum.TryParse<DifficultyEnum>(updateDto.Difficulty, true, out var difficulty) ? difficulty : null);

            quest.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            quest.SetLabels(updateDto.Labels);

            switch (quest.QuestType)
            {
                case QuestTypeEnum.Weekly:
                {
                    var updateWeeklyDto = (UpdateWeeklyQuestDto)updateDto;
                    quest.SetWeekdays(updateWeeklyDto.Weekdays.Select(d => Enum.Parse<WeekdayEnum>(d)));
                    break;
                }
                case QuestTypeEnum.Monthly:
                {
                    var updateMonthlyDto = (UpdateMonthlyQuestDto)updateDto;
                    quest.SetMonthlyDays(updateMonthlyDto.StartDay, updateMonthlyDto.EndDay);
                    break;
                }
                case QuestTypeEnum.Seasonal:
                {
                    var updateSeasonalDto = (UpdateSeasonalQuestDto)updateDto;
                    quest.SetSeason(Enum.Parse<SeasonEnum>(updateSeasonalDto.Season));
                    break;
                }
            }

            var now = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            if (quest.IsRepeatable())
            {
                quest.SetNextResetAt(questResetService);
                if (await _unitOfWork.QuestOccurrences.GetCurrentOccurrenceForQuestAsync(quest.Id, now, cancellationToken).ConfigureAwait(false) is null)
                {
                    var generatedOccurrences = await _questOccurrenceGenerator.GenerateMissingOccurrencesForQuestAsync(quest, cancellationToken).ConfigureAwait(false);
                    if (generatedOccurrences.Count != 0)
                    {
                        quest.AddOccurrences(generatedOccurrences);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return _questMappingService.MapToDto(quest);
        }
    }
}
