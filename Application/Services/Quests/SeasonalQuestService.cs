using Application.Dtos.Quests.SeasonalQuest;
using Application.Interfaces;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces.Quests;
using Domain.Models;

namespace Application.Services.Quests
{
    public class SeasonalQuestService : ISeasonalQuestService
    {
        private readonly ISeasonalQuestRepository _repository;
        private readonly IQuestMetadataRepository _questMetadataRepository;
        private readonly IMapper _mapper;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public SeasonalQuestService(
            ISeasonalQuestRepository repository,
            IMapper mapper,
            IQuestMetadataRepository questMetadataRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _repository = repository;
            _mapper = mapper;
            _questMetadataRepository = questMetadataRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetSeasonalQuestDto?> GetUserQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestByIdAsync(questId, cancellationToken);

            if (quest is null)
                return null;

            if (quest.QuestType != QuestTypeEnum.Seasonal)
                throw new InvalidQuestTypeException(questId, QuestTypeEnum.Seasonal, quest.QuestType);

            return _mapper.Map<GetSeasonalQuestDto>(quest);
        }

        public async Task<IEnumerable<GetSeasonalQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questMetadataRepository.GetQuestsByTypeAsync(accountId, QuestTypeEnum.Seasonal, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetSeasonalQuestDto>>(quests);
        }
        public async Task<int> CreateAsync(CreateSeasonalQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var seasonalQuest = _mapper.Map<SeasonalQuest>(createDto);

            await _repository.AddAsync(seasonalQuest, cancellationToken);

            return seasonalQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, UpdateSeasonalQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questMetadataRepository.GetQuestByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (existingQuest.QuestType != QuestTypeEnum.Seasonal)
                throw new InvalidQuestTypeException(id, QuestTypeEnum.Seasonal, existingQuest.QuestType);

            existingQuest.SeasonalQuest!.UpdateDates(updateDto.StartDate, updateDto.EndDate, false);

            _mapper.Map(updateDto, existingQuest.SeasonalQuest);

            var questWithLabels = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest.SeasonalQuest = questWithLabels.SeasonalQuest!;

            await _repository.UpdateAsync(existingQuest.SeasonalQuest, cancellationToken);
        }

        public async Task PatchUserQuestAsync(int id, PatchSeasonalQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingSeasonalQuest = await _repository.GetByIdAsync(id, cancellationToken, sq => sq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            existingSeasonalQuest.UpdateDates(patchDto.StartDate, patchDto.EndDate, false);

            _mapper.Map(patchDto, existingSeasonalQuest);

            await _repository.UpdateAsync(existingSeasonalQuest, cancellationToken);
        }
    }
}
