using Application.Dtos.MonthlyQuest;
using Application.Interfaces;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Services
{
    public class MonthlyQuestService : IMonthlyQuestService
    {
        private readonly IMonthlyQuestRepository _repository;
        private readonly IMapper _mapper;

        public MonthlyQuestService(IMonthlyQuestRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<MonthlyQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<MonthlyQuestDto>(quest);
        }

        public async Task<IEnumerable<MonthlyQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<MonthlyQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateMonthlyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var monthlyQuest = _mapper.Map<MonthlyQuest>(createDto);

            monthlyQuest.QuestMetadata.Id = monthlyQuest.Id;
            monthlyQuest.QuestMetadata.AccountId = createDto.AccountId;
            monthlyQuest.QuestMetadata.QuestType = QuestTypeEnum.Monthly;

            await _repository.AddAsync(monthlyQuest, cancellationToken);

            return monthlyQuest.Id;
        }

        public async Task UpdateAsync(int id, UpdateMonthlyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingDailyQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            _mapper.Map(updateDto, existingDailyQuest);

            await _repository.UpdateAsync(existingDailyQuest, cancellationToken);
        }

        public async Task PatchAsync(int id, PatchMonthlyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingMonthlyQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            // **Fix: Manually Preserve IsCompleted Before AutoMapper Mapping**
            bool previousIsCompleted = existingMonthlyQuest.IsCompleted;

            // Apply AutoMapper Mapping (Ignores Nulls)
            _mapper.Map(patchDto, existingMonthlyQuest);

            // **Fix: Restore IsCompleted If Not Provided**
            if (patchDto.IsCompleted is null)
            {
                existingMonthlyQuest.IsCompleted = previousIsCompleted;
            }

            await _repository.UpdateAsync(existingMonthlyQuest, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            await _repository.DeleteAsync(quest, cancellationToken);
        }
    }
}
