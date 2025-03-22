using Application.Dtos.Quests.WeeklyQuest;
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
    public class WeeklyQuestService : IWeeklyQuestService
    {
        private readonly IWeeklyQuestRepository _repository;
        private readonly IQuestMetadataRepository _questMetadataRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<WeeklyQuestService> _logger;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public WeeklyQuestService(
            IWeeklyQuestRepository repository,
            IMapper mapper,
            ILogger<WeeklyQuestService> logger,
            IQuestMetadataRepository questMetadataRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
            _questMetadataRepository = questMetadataRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetWeeklyQuestDto?> GetUserQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestByIdAsync(questId, cancellationToken);

            if (quest is null)
                return null;

            if (quest.QuestType != QuestTypeEnum.Weekly)
                throw new InvalidQuestTypeException(questId, QuestTypeEnum.Weekly, quest.QuestType);

            return _mapper.Map<GetWeeklyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetWeeklyQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questMetadataRepository.GetQuestsByTypeAsync(accountId, QuestTypeEnum.Weekly, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetWeeklyQuestDto>>(quests);
        }
        public async Task<int> CreateAsync(CreateWeeklyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var weeklyQuest = _mapper.Map<WeeklyQuest>(createDto);

            await _repository.AddAsync(weeklyQuest, cancellationToken);

            return weeklyQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, UpdateWeeklyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questMetadataRepository.GetQuestByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            // Check if ONLY StartDate is being updated and ensure it's still valid with the existing EndDate
            if (updateDto.StartDate.HasValue && existingQuest.WeeklyQuest!.EndDate.HasValue)
            {
                if (updateDto.StartDate.Value > existingQuest.WeeklyQuest.EndDate.Value)
                    throw new InvalidArgumentException("Start date cannot be after the existing end date.");
            }

            // Check if ONLY EndDate is being updated and ensure it's still valid with the existing StartDate
            if (updateDto.EndDate.HasValue && existingQuest.WeeklyQuest!.StartDate.HasValue)
            {
                if (updateDto.EndDate.Value < existingQuest.WeeklyQuest.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before the existing start date.");
            }

            _mapper.Map(updateDto, existingQuest.WeeklyQuest);

            var questWithLabels = await _questLabelsHandler.HandlePatchLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest.WeeklyQuest = questWithLabels.WeeklyQuest!;

            await _repository.UpdateAsync(existingQuest.WeeklyQuest, cancellationToken);
        }

        public async Task PatchUserQuestAsync(int id, PatchWeeklyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _repository.GetByIdAsync(id, cancellationToken, wq => wq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            // Check if ONLY StartDate is being updated and ensure it's still valid with the existing EndDate
            if (patchDto.StartDate.HasValue && existingQuest.EndDate.HasValue)
            {
                if (patchDto.StartDate.Value > existingQuest.EndDate.Value)
                    throw new InvalidArgumentException("Start date cannot be after the existing end date.");
            }

            // Check if ONLY EndDate is being updated and ensure it's still valid with the existing StartDate
            if (patchDto.EndDate.HasValue && existingQuest.StartDate.HasValue)
            {
                if (patchDto.EndDate.Value < existingQuest.StartDate.Value)
                    throw new InvalidArgumentException("End date cannot be before the existing start date.");
            }

            // **Fix: Manually Preserve WeekDays Before AutoMapper Mapping**
            List<WeekdayEnum> previousWeekdays = existingQuest.Weekdays;

            _mapper.Map(patchDto, existingQuest);

            // **Fix: Restore Weekdays If Not Provided**
            if (patchDto.Weekdays is null)
                existingQuest.Weekdays = previousWeekdays;

            await _repository.UpdateAsync(existingQuest, cancellationToken);
        }

        public async Task DeleteUserQuestAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestMetadataByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            await _questMetadataRepository.DeleteAsync(quest, cancellationToken);
        }
    }
}
