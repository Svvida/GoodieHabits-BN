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
        private readonly IDailyQuestRepository _repository;
        private readonly IQuestMetadataRepository _questMetadataRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DailyQuestService> _logger;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public DailyQuestService(
            IDailyQuestRepository repository,
            IMapper mapper,
            ILogger<DailyQuestService> logger,
            IQuestMetadataRepository questMetadataRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _questMetadataRepository = questMetadataRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetDailyQuestDto?> GetUserQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestByIdAsync(questId, cancellationToken);

            if (quest is null)
                return null;

            if (quest.QuestType != QuestTypeEnum.Daily)
                throw new InvalidQuestTypeException(questId, QuestTypeEnum.Daily, quest.QuestType);

            return _mapper.Map<GetDailyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetDailyQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questMetadataRepository.GetQuestsByTypeAsync(accountId, QuestTypeEnum.Daily, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetDailyQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateDailyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var dailyQuest = _mapper.Map<DailyQuest>(createDto);

            _logger.LogInformation("DailyQuest created after mapping: {@dailyQuest}", dailyQuest);

            await _repository.AddAsync(dailyQuest, cancellationToken);

            return dailyQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, UpdateDailyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questMetadataRepository.GetQuestByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (existingQuest.QuestType != QuestTypeEnum.Daily)
                throw new InvalidQuestTypeException(id, QuestTypeEnum.Daily, existingQuest.QuestType);

            existingQuest.DailyQuest!.UpdateDates(updateDto.StartDate, updateDto.EndDate, false);

            _mapper.Map(updateDto, existingQuest.DailyQuest);

            var questWithLabels = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest.DailyQuest = questWithLabels.DailyQuest!;

            await _repository.UpdateAsync(existingQuest.DailyQuest, cancellationToken);
        }

        public async Task PatchUserQuestAsync(int id, PatchDailyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingDailyQuest = await _repository.GetByIdAsync(id, cancellationToken, dq => dq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            existingDailyQuest.UpdateDates(patchDto.StartDate, patchDto.EndDate, false);

            if (existingDailyQuest.IsCompleted == false && patchDto.IsCompleted == true)
                existingDailyQuest.LastCompleted = DateTime.UtcNow;

            _mapper.Map(patchDto, existingDailyQuest);

            await _repository.UpdateAsync(existingDailyQuest, cancellationToken);
        }
    }
}
