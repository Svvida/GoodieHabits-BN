using Application.Dtos.Quests.DailyQuest;
using Application.Interfaces;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces.Quests;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class DailyQuestService : IDailyQuestService
    {
        private readonly IQuestRepository _questRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DailyQuestService> _logger;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public DailyQuestService(
            IMapper mapper,
            ILogger<DailyQuestService> logger,
            IQuestRepository questRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _mapper = mapper;
            _logger = logger;
            _questRepository = questRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetDailyQuestDto?> GetUserQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await _questRepository.GetQuestByIdAsync(questId, QuestTypeEnum.Daily, cancellationToken).ConfigureAwait(false);

            if (quest is null)
                return null;

            //if (quest.QuestType != QuestTypeEnum.Daily)
            //    throw new InvalidQuestTypeException(questId, QuestTypeEnum.Daily, quest.QuestType);

            return _mapper.Map<GetDailyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetDailyQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questRepository.GetQuestsByTypeAsync(accountId, QuestTypeEnum.Daily, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetDailyQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateDailyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var dailyQuest = _mapper.Map<Quest>(createDto);

            _logger.LogInformation("DailyQuest created after mapping: {@dailyQuest}", dailyQuest);

            await _questRepository.AddQuestAsync(dailyQuest, cancellationToken);

            return dailyQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, UpdateDailyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questRepository.GetQuestByIdAsync(id, QuestTypeEnum.Daily, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            //if (existingQuest.QuestType != QuestTypeEnum.Daily)
            //    throw new InvalidQuestTypeException(id, QuestTypeEnum.Daily, existingQuest.QuestType);

            existingQuest.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            _mapper.Map(updateDto, existingQuest);

            var questWithLabels = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest = questWithLabels;

            await _questRepository.UpdateQuestAsync(existingQuest, cancellationToken);
        }

        public async Task UpdateQuestCompletionAsync(int id, DailyQuestCompletionPatchDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingDailyQuest = await _questRepository.GetQuestByIdAsync(id, QuestTypeEnum.Daily, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            if (existingDailyQuest.IsCompleted == false && patchDto.IsCompleted == true)
                existingDailyQuest.LastCompletedAt = DateTime.UtcNow;

            _mapper.Map(patchDto, existingDailyQuest);

            await _questRepository.UpdateQuestAsync(existingDailyQuest, cancellationToken);
        }
    }
}
