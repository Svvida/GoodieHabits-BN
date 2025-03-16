using Application.Dtos.Quests.DailyQuest;
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
    public class DailyQuestService : IDailyQuestService
    {
        private readonly IDailyQuestRepository _repository;
        private readonly IQuestMetadataRepository _questMetadataRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DailyQuestService> _logger;
        private readonly IQuestLabelService _questLabelService;
        private readonly IQuestLabelRepository _questLabelRepository;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public DailyQuestService(
            IDailyQuestRepository repository,
            IMapper mapper,
            ILogger<DailyQuestService> logger,
            IQuestMetadataRepository questMetadataRepository,
            IQuestLabelService questLabelService,
            IQuestLabelRepository questLabelRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _questMetadataRepository = questMetadataRepository;
            _questLabelService = questLabelService;
            _questLabelRepository = questLabelRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetDailyQuestDto?> GetUserQuestByIdAsync(int questId, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestByIdAsync(questId, cancellationToken);

            if (quest is null)
                return null;

            if (quest.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

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

        public async Task UpdateUserQuestAsync(int id, int accountId, UpdateDailyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questMetadataRepository.GetQuestByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            _logger.LogInformation("DailyQuest before update: {@existingQuest}", existingQuest);

            if (existingQuest.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            // Check if ONLY StartDate is being updated and ensure it's still valid with the existing EndDate
            if (updateDto.StartDate.HasValue && existingQuest.DailyQuest!.EndDate.HasValue)
            {
                if (updateDto.StartDate.Value > existingQuest.DailyQuest.EndDate.Value)
                    throw new InvalidArgumentException("Start date cannot be after the existing end date.");
            }

            // Check if ONLY EndDate is being updated and ensure it's still valid with the existing StartDate
            if (updateDto.EndDate.HasValue && existingQuest.DailyQuest!.StartDate.HasValue)
            {
                if (updateDto.EndDate.Value < existingQuest.DailyQuest.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before the existing start date.");
            }

            _mapper.Map(updateDto, existingQuest.DailyQuest);

            var questWithLabels = await _questLabelsHandler.HandlePatchLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest.DailyQuest = questWithLabels.DailyQuest!;

            await _repository.UpdateAsync(existingQuest.DailyQuest, cancellationToken);
        }

        public async Task PatchUserQuestAsync(int id, int accountId, PatchDailyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingDailyQuest = await _repository.GetByIdAsync(id, cancellationToken, dq => dq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            if (existingDailyQuest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            // Check if ONLY StartDate is being updated and ensure it's still valid with the existing EndDate
            if (patchDto.StartDate.HasValue && existingDailyQuest.EndDate.HasValue)
            {
                if (patchDto.StartDate.Value > existingDailyQuest.EndDate.Value)
                    throw new InvalidArgumentException("Start date cannot be after the existing end date.");
            }

            // Check if ONLY EndDate is being updated and ensure it's still valid with the existing StartDate
            if (patchDto.EndDate.HasValue && existingDailyQuest.StartDate.HasValue)
            {
                if (patchDto.EndDate.Value < existingDailyQuest.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before the existing start date.");
            }

            if (existingDailyQuest.IsCompleted == false && patchDto.IsCompleted == true)
                existingDailyQuest.LastCompleted = DateTime.UtcNow;

            _mapper.Map(patchDto, existingDailyQuest);

            await _repository.UpdateAsync(existingDailyQuest, cancellationToken);
        }

        public async Task DeleteUserQuestAsync(int id, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestMetadataByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (quest.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            await _questMetadataRepository.DeleteAsync(quest, cancellationToken);
        }
    }
}
