using Application.Dtos.DailyQuest;
using Application.Interfaces;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Services
{
    public class DailyQuestService : IDailyQuestService
    {
        private readonly IDailyQuestRepository _repository;
        private readonly IMapper _mapper;

        public DailyQuestService(IDailyQuestRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<DailyQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<DailyQuestDto>(quest);
        }

        public async Task<IEnumerable<DailyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<DailyQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateDailyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var dailyQuest = _mapper.Map<DailyQuest>(createDto);

            dailyQuest.QuestMetadata.Id = dailyQuest.Id;
            dailyQuest.QuestMetadata.AccountId = createDto.AccountId;
            dailyQuest.QuestMetadata.QuestType = QuestTypeEnum.Daily;

            await _repository.AddAsync(dailyQuest, cancellationToken);

            return dailyQuest.Id;
        }

        public async Task UpdateAsync(int id, UpdateDailyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingDailyQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            _mapper.Map(updateDto, existingDailyQuest);

            await _repository.UpdateAsync(existingDailyQuest, cancellationToken);
        }

        public async Task PatchAsync(int id, PatchDailyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingDailyQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            // **Fix: Manually Preserve IsCompleted Before AutoMapper Mapping**
            bool previousIsCompleted = existingDailyQuest.IsCompleted;

            // Apply AutoMapper Mapping (Ignores Nulls)
            _mapper.Map(patchDto, existingDailyQuest);

            // **Fix: Restore IsCompleted If Not Provided**
            if (patchDto.IsCompleted is null)
                existingDailyQuest.IsCompleted = previousIsCompleted;

            await _repository.UpdateAsync(existingDailyQuest, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            await _repository.DeleteAsync(quest, cancellationToken);
        }
    }
}
