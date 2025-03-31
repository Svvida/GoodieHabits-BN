using Application.Dtos.Quests;
using Application.Dtos.Quests.DailyQuest;
using Application.Dtos.Quests.MonthlyQuest;
using Application.Dtos.Quests.OneTimeQuest;
using Application.Dtos.Quests.SeasonalQuest;
using Application.Dtos.Quests.WeeklyQuest;
using Application.Helpers;
using Application.Interfaces;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Interfaces.Quests;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class QuestService : IQuestService
    {
        private readonly IQuestRepository _questRepository;
        private readonly IQuestLabelsHandler _questLabelsHandler;
        private readonly IQuestWeekdaysHandler _questWeekdaysHandler;
        private readonly IMapper _mapper;
        private readonly ILogger<QuestService> _logger;
        private readonly IQuestLabelRepository _questLabelRepository;

        public QuestService(
            IQuestRepository repository,
            IQuestLabelsHandler questLabelsHandler,
            IQuestWeekdaysHandler questWeekdaysHandler,
            IMapper mapper,
            ILogger<QuestService> logger,
            IQuestLabelRepository questLabelRepository)
        {
            _questRepository = repository;
            _questLabelsHandler = questLabelsHandler;
            _questWeekdaysHandler = questWeekdaysHandler;
            _mapper = mapper;
            _logger = logger;
            _questLabelRepository = questLabelRepository;
        }

        public async Task<BaseGetQuestDto?> GetUserQuestByIdAsync(int questId, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var quest = await _questRepository.GetQuestByIdAsync(questId, questType, cancellationToken);

            if (quest is null)
                return null;

            return MapToDto(quest);
        }

        public async Task<IEnumerable<BaseGetQuestDto>> GetAllUserQuestsByTypeAsync(int accountId, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var quests = await _questRepository.GetQuestsByTypeAsync(accountId, questType, cancellationToken)
                .ConfigureAwait(false);
            return quests.Select(MapToDto);
        }
        public async Task<int> CreateUserQuestAsync(BaseCreateQuestDto createDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            foreach (var label in createDto.Labels)
            {
                var isOwner = await _questLabelRepository.IsLabelOwnedByUserAsync(label, createDto.AccountId, cancellationToken)
                    .ConfigureAwait(false);
                if (!isOwner)
                    throw new ForbiddenException($"Label with ID: {label} does not belong to the user.");
            }

            var quest = _mapper.Map<Quest>(createDto);
            _logger.LogInformation("Quest created after mapping: {@quest}", quest);
            await _questRepository.AddQuestAsync(quest, cancellationToken);
            return quest.Id;
        }

        public async Task UpdateUserQuestAsync(BaseUpdateQuestDto updateDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Type of updateDto from controller: {@updateDto}", updateDto);
            var existingQuest = await _questRepository.GetQuestByIdAsync(updateDto.Id, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {updateDto.Id} not found");

            existingQuest.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            existingQuest = _mapper.Map(updateDto, existingQuest);
            _logger.LogInformation("Quest updated after mapping: {@quest}", existingQuest);

            existingQuest = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);

            if (questType == QuestTypeEnum.Weekly)
            {
                var weeklyUpdateDto = (UpdateWeeklyQuestDto)updateDto;
                existingQuest = _questWeekdaysHandler.HandleUpdateWeekdays(existingQuest, weeklyUpdateDto);
                existingQuest.NextResetAt = QuestResetCalculator.GetNextResetTimeUtc(existingQuest, _logger);
            }

            if (questType == QuestTypeEnum.Monthly)
                existingQuest.NextResetAt = QuestResetCalculator.GetNextResetTimeUtc(existingQuest, _logger);

            await _questRepository.UpdateQuestAsync(existingQuest, cancellationToken);
        }

        public async Task UpdateQuestCompletionAsync(BaseQuestCompletionPatchDto patchDto, QuestTypeEnum questType, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questRepository.GetQuestByIdAsync(patchDto.Id, questType, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with ID: {patchDto.Id} not found");

            if (existingQuest.IsCompleted == false && patchDto.IsCompleted == true)
            {
                existingQuest.LastCompletedAt = DateTime.UtcNow;
                existingQuest.NextResetAt = QuestResetCalculator.GetNextResetTimeUtc(existingQuest, _logger);
            }

            existingQuest = _mapper.Map(patchDto, existingQuest);

            _logger.LogInformation("Completed quest after mapping: {@existingQuest}", existingQuest);

            await _questRepository.UpdateQuestAsync(existingQuest, cancellationToken);
        }

        public async Task<IEnumerable<BaseGetQuestDto>> GetActiveQuestsAsync(
            int accountId, CancellationToken cancellationToken = default)
        {
            SeasonEnum currentSeason = SeasonHelper.GetCurrentSeason();

            var quests = await _questRepository.GetActiveQuestsAsync(accountId, currentSeason, cancellationToken)
                .ConfigureAwait(false);

            _logger.LogInformation("Quests before mapping: {@quests}", quests);

            var mappedQuests = quests.Select(MapToDto);

            _logger.LogInformation("Quests after mapping: {@mappedQuests}", mappedQuests);

            return mappedQuests;
        }

        public async Task DeleteQuestAsync(int questId, CancellationToken cancellationToken = default)
        {
            await _questRepository.DeleteQuestByIdAsync(questId, cancellationToken).ConfigureAwait(false);
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

