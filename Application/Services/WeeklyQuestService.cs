using Application.Dtos.WeeklyQuest;
using Application.Helpers;
using Application.Interfaces;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Services
{
    public class WeeklyQuestService : IWeeklyQuestService
    {
        private readonly IWeeklyQuestRepository _repository;
        private readonly IMapper _mapper;

        public WeeklyQuestService(IWeeklyQuestRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<WeeklyQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<WeeklyQuestDto>(quest);
        }

        public async Task<IEnumerable<WeeklyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<WeeklyQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateWeeklyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            QuestValidationHelper.ValidateWeekdays(createDto.Weekdays);

            var weeklyQuest = _mapper.Map<WeeklyQuest>(createDto);

            weeklyQuest.QuestMetadata.Id = weeklyQuest.Id;
            weeklyQuest.QuestMetadata.AccountId = createDto.AccountId;
            weeklyQuest.QuestMetadata.QuestType = QuestTypeEnum.Weekly;

            await _repository.AddAsync(weeklyQuest, cancellationToken);

            return weeklyQuest.Id;
        }

        public async Task UpdateAsync(int id, UpdateWeeklyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            if (updateDto.Weekdays is not null)
                QuestValidationHelper.ValidateWeekdays(updateDto.Weekdays);

            var existingQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            _mapper.Map(updateDto, existingQuest);

            await _repository.UpdateAsync(existingQuest, cancellationToken);
        }

        public async Task PatchAsync(int id, PatchWeeklyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            if (patchDto.Weekdays is not null)
                QuestValidationHelper.ValidateWeekdays(patchDto.Weekdays);

            var existingQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            // **Fix: Manually Preserve IsCompleted Before AutoMapper Mapping**
            bool previousIsCompleted = existingQuest.IsCompleted;
            List<WeekdayEnum> previousWeekdays = existingQuest.Weekdays;

            // Apply AutoMapper Mapping (Ignores Nulls)
            _mapper.Map(patchDto, existingQuest);

            // **Fix: Restore IsCompleted If Not Provided**
            if (patchDto.IsCompleted is null)
                existingQuest.IsCompleted = previousIsCompleted;
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
