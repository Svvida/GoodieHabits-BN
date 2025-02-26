using Application.Dtos.Quests.WeeklyQuest;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Services.Quests
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

        public async Task<GetWeeklyQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<GetWeeklyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetWeeklyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

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

        public async Task UpdateAsync(int id, UpdateWeeklyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            _mapper.Map(updateDto, existingQuest);

            await _repository.UpdateAsync(existingQuest, cancellationToken);
        }

        public async Task PatchAsync(int id, PatchWeeklyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _repository.GetByIdAsync(id, cancellationToken)
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

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            await _repository.DeleteAsync(quest, cancellationToken);
        }
    }
}
