using Application.Dtos.SeasonalQuest;
using Application.Interfaces;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;

namespace Application.Services
{
    public class SeasonalQuestService : ISeasonalQuestService
    {
        private readonly ISeasonalQuestRepository _repository;
        private readonly IMapper _mapper;

        public SeasonalQuestService(ISeasonalQuestRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<SeasonalQuestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken);

            return quest is null ? null : _mapper.Map<SeasonalQuestDto>(quest);
        }

        public async Task<IEnumerable<SeasonalQuestDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var quests = await _repository.GetAllAsync(cancellationToken);

            return _mapper.Map<IEnumerable<SeasonalQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateSeasonalQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var seasonalQuest = _mapper.Map<SeasonalQuest>(createDto);

            seasonalQuest.QuestMetadata.Id = seasonalQuest.Id;
            seasonalQuest.QuestMetadata.AccountId = createDto.AccountId;
            seasonalQuest.QuestMetadata.QuestType = QuestTypeEnum.Seasonal;

            await _repository.AddAsync(seasonalQuest, cancellationToken);

            return seasonalQuest.Id;
        }

        public async Task UpdateAsync(int id, UpdateSeasonalQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingSeasonalQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            _mapper.Map(updateDto, existingSeasonalQuest);

            await _repository.UpdateAsync(existingSeasonalQuest, cancellationToken);
        }

        public async Task PatchAsync(int id, PatchSeasonalQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingSeasonalQuest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            // **Fix: Manually Preserve IsCompleted Before AutoMapper Mapping**
            bool previousIsCompleted = existingSeasonalQuest.IsCompleted;

            // Apply AutoMapper Mapping (Ignores Nulls)
            _mapper.Map(patchDto, existingSeasonalQuest);

            // **Fix: Restore IsCompleted If Not Provided**
            if (patchDto.IsCompleted is null)
                existingSeasonalQuest.IsCompleted = previousIsCompleted;


            await _repository.UpdateAsync(existingSeasonalQuest, cancellationToken);
        }

        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var quest = await _repository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"DailyQuest with Id {id} was not found.");

            await _repository.DeleteAsync(quest, cancellationToken);
        }
    }
}
