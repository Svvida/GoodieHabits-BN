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
        private readonly IQuestRepository _questRepository;
        private readonly IMapper _mapper;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public SeasonalQuestService(
            IMapper mapper,
            IQuestRepository questRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _mapper = mapper;
            _questRepository = questRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetSeasonalQuestDto?> GetUserQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await _questRepository.GetQuestByIdAsync(questId, QuestTypeEnum.Seasonal, cancellationToken);

            if (quest is null)
                return null;

            //if (quest.QuestType != QuestTypeEnum.Seasonal)
            //    throw new InvalidQuestTypeException(questId, QuestTypeEnum.Seasonal, quest.QuestType);

            return _mapper.Map<GetSeasonalQuestDto>(quest);
        }

        public async Task<IEnumerable<GetSeasonalQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questRepository.GetQuestsByTypeAsync(accountId, QuestTypeEnum.Seasonal, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetSeasonalQuestDto>>(quests);
        }
        public async Task<int> CreateAsync(CreateSeasonalQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var seasonalQuest = _mapper.Map<Quest>(createDto);

            await _questRepository.AddQuestAsync(seasonalQuest, cancellationToken);

            return seasonalQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, UpdateSeasonalQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questRepository.GetQuestByIdAsync(id, QuestTypeEnum.Seasonal, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            //if (existingQuest.QuestType != QuestTypeEnum.Seasonal)
            //    throw new InvalidQuestTypeException(id, QuestTypeEnum.Seasonal, existingQuest.QuestType);

            existingQuest.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            _mapper.Map(updateDto, existingQuest);

            var questWithLabels = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest = questWithLabels;

            await _questRepository.UpdateQuestAsync(existingQuest, cancellationToken);
        }

        public async Task UpdateQuestCompletionAsync(int id, SeasonalQuestCompletionPatchDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingSeasonalQuest = await _questRepository.GetQuestByIdAsync(id, QuestTypeEnum.Seasonal, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            _mapper.Map(patchDto, existingSeasonalQuest);

            await _questRepository.UpdateQuestAsync(existingSeasonalQuest, cancellationToken);
        }
    }
}
