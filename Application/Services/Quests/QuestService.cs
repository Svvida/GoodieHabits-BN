using Application.Dtos.Quests;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Application.Services.Quests
{
    public class QuestService : IQuestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestService> _logger;
        private readonly IQuestResetService _questResetService;
        private readonly IQuestOccurrenceGenerator _questOccurrenceGenerator;

        public QuestService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<QuestService> logger,
            IQuestResetService questResetService,
            IQuestOccurrenceGenerator questOccurrenceGenerator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _questResetService = questResetService;
            _questOccurrenceGenerator = questOccurrenceGenerator;
        }

        public async Task<BaseGetQuestDto> UpdateUserQuestAsync(BaseUpdateQuestDto updateDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            // Check if the account is owner of the labels
            if (updateDto.Labels.Count > 0)
            {
                int ownedCount = await _unitOfWork.QuestLabels.CountOwnedLabelsAsync(updateDto.Labels, updateDto.AccountId, cancellationToken).ConfigureAwait(false);

                if (ownedCount != updateDto.Labels.Count)
                {
                    _logger.LogWarning("User {AccountId} tried to create a quest with labels they do not own.", updateDto.AccountId);
                    throw new ForbiddenException("One or more provided labels do not exist or do not belong to the user.");
                }
            }

            var existingQuest = await _unitOfWork.Quests.GetQuestByIdAsync(updateDto.Id, questType, false, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {updateDto.Id} not found");

            existingQuest.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            existingQuest = _mapper.Map(updateDto, existingQuest);

            // Update labels if provided
            var existingLabels = existingQuest.Quest_QuestLabels.ToList();

            HashSet<int> existingLabelsHashSet = [.. existingQuest.Quest_QuestLabels.Select(x => x.QuestLabelId)];
            HashSet<int> newLabelsHashSet = [.. updateDto.Labels];

            _logger.LogDebug("Existing labels: {@existingLabels}", existingLabels);
            _logger.LogDebug("New labels: {@newLabels}", newLabelsHashSet);

            var labelsToAdd = updateDto.Labels
                .Where(labelId => !existingLabelsHashSet.Contains(labelId))
                .Select(labelId => new Quest_QuestLabel
                {
                    QuestId = existingQuest.Id,
                    QuestLabelId = labelId
                }).ToList();

            var labelsToRemove = existingLabels
                .Where(existingLabel => !newLabelsHashSet.Contains(existingLabel.QuestLabelId))
                .ToList();

            foreach (var label in labelsToAdd)
            {
                existingQuest.Quest_QuestLabels.Add(label);
            }

            foreach (var label in labelsToRemove)
            {
                existingQuest.Quest_QuestLabels.Remove(label);
            }

            var now = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
            if (existingQuest.IsRepeatable())
            {
                existingQuest.NextResetAt = _questResetService.GetNextResetTimeUtc(existingQuest);
                if (await _unitOfWork.QuestOccurrences.GetCurrentOccurrenceForQuestAsync(existingQuest.Id, now, cancellationToken).ConfigureAwait(false) is null)
                {
                    var generatedOccurrences = await _questOccurrenceGenerator.GenerateMissingOccurrencesForQuestAsync(existingQuest, cancellationToken).ConfigureAwait(false);
                    if (generatedOccurrences.Count != 0)
                    {
                        foreach (var generatedOccurrence in generatedOccurrences)
                        {
                            _logger.LogDebug("Adding generated occurrence: {@generatedOccurrence}", generatedOccurrence);
                            existingQuest.QuestOccurrences.Add(generatedOccurrence);
                        }
                    }
                }
                existingQuest.Statistics = await _unitOfWork.QuestStatistics.GetStatisticsForQuestAsync(existingQuest.Id, true, cancellationToken).ConfigureAwait(false);
            }

            if (questType == QuestTypeEnum.Weekly)
            {
                var weeklyUpdateDto = (UpdateWeeklyQuestDto)updateDto;

                var desiredWeekdays = weeklyUpdateDto.Weekdays.Select(wd => Enum.Parse<WeekdayEnum>(wd)).ToHashSet();
                var existingWeekdays = existingQuest.WeeklyQuest_Days.Select(wqd => wqd.Weekday).ToHashSet();

                var weekdaysToAdd = desiredWeekdays.Except(existingWeekdays);
                foreach (var weekday in weekdaysToAdd)
                {
                    existingQuest.WeeklyQuest_Days.Add(new WeeklyQuest_Day
                    {
                        Weekday = weekday
                    });
                }

                var weekdaysToRemove = existingQuest.WeeklyQuest_Days
                    .Where(wqd => !desiredWeekdays.Contains(wqd.Weekday))
                    .ToList();
                foreach (var weekday in weekdaysToRemove)
                {
                    existingQuest.WeeklyQuest_Days.Remove(weekday);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return MapToDto(existingQuest);

        }

        private BaseGetQuestDto MapToDto(Quest quest)
        {
            return quest.QuestType switch
            {
                QuestTypeEnum.OneTime => _mapper.Map<GetOneTimeQuestDto>(quest),
                QuestTypeEnum.Daily => _mapper.Map<GetDailyQuestDto>(quest),
                QuestTypeEnum.Weekly => _mapper.Map<GetWeeklyQuestDto>(quest),
                QuestTypeEnum.Monthly => _mapper.Map<GetMonthlyQuestDto>(quest),
                QuestTypeEnum.Seasonal => _mapper.Map<GetSeasonalQuestDto>(quest),
                _ => throw new InvalidArgumentException("Invalid quest type")
            };
        }
    }
}
