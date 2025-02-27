using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class WeeklyQuestService : IWeeklyQuestService
    {
        private readonly IWeeklyQuestRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<WeeklyQuestService> _logger;

        public WeeklyQuestService(
            IWeeklyQuestRepository repository,
            IMapper mapper,
            ILogger<WeeklyQuestService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<GetWeeklyQuestDto?> GetQuestByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<GetWeeklyQuestDto>(quest);
        }

        public async Task<GetWeeklyQuestDto?> GetUserQuestByIdAsync(int questId, int accountId, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(questId, cancellationToken, wq => wq.QuestMetadata);

            if (quest is null)
                return null;

            if (quest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            return _mapper.Map<GetWeeklyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetWeeklyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<GetWeeklyQuestDto>>(quests);
        }
        public async Task<IEnumerable<GetWeeklyQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllUserQuestsAsync(accountId, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetWeeklyQuestDto>>(quests);
        }
        public async Task<int> CreateAsync(CreateWeeklyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var weeklyQuest = _mapper.Map<WeeklyQuest>(createDto);

            weeklyQuest.QuestMetadata.Id = weeklyQuest.Id;
            weeklyQuest.QuestMetadata.AccountId = createDto.AccountId;
            weeklyQuest.QuestMetadata.QuestType = QuestTypeEnum.Weekly;

            await _repository.AddAsync(weeklyQuest, cancellationToken);

            return weeklyQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, int accountId, UpdateWeeklyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _repository.GetByIdAsync(id, cancellationToken, wq => wq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (existingQuest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

            _mapper.Map(updateDto, existingQuest);

            await _repository.UpdateAsync(existingQuest, cancellationToken);
        }

        public async Task PatchUserQuestAsync(int id, int accountId, PatchWeeklyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _repository.GetByIdAsync(id, cancellationToken, wq => wq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            if (existingQuest.QuestMetadata.AccountId != accountId)
                throw new UnauthorizedException("You do not have permission to access this quest.");

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

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            await _repository.DeleteAsync(quest, cancellationToken);
        }
    }
}
