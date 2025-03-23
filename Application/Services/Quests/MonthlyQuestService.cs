using Application.Dtos.Quests.MonthlyQuest;
using Application.Interfaces;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces.Quests;
using Domain.Models;

namespace Application.Services.Quests
{
    public class MonthlyQuestService : IMonthlyQuestService
    {
        private readonly IMonthlyQuestRepository _repository;
        private readonly IMapper _mapper;
        private readonly IQuestMetadataRepository _questMetadataRepository;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public MonthlyQuestService(
            IMonthlyQuestRepository repository,
            IMapper mapper,
            IQuestMetadataRepository questMetadataRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _repository = repository;
            _mapper = mapper;
            _questMetadataRepository = questMetadataRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetMonthlyQuestDto?> GetUserQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await _questMetadataRepository.GetQuestByIdAsync(questId, cancellationToken);

            if (quest is null)
                return null;

            if (quest.QuestType != QuestTypeEnum.Monthly)
                throw new InvalidQuestTypeException(questId, QuestTypeEnum.Monthly, quest.QuestType);

            return _mapper.Map<GetMonthlyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetMonthlyQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questMetadataRepository.GetQuestsByTypeAsync(accountId, QuestTypeEnum.Monthly, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetMonthlyQuestDto>>(quests);
        }
        public async Task<int> CreateAsync(CreateMonthlyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var monthlyQuest = _mapper.Map<MonthlyQuest>(createDto);

            await _repository.AddAsync(monthlyQuest, cancellationToken);

            return monthlyQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, UpdateMonthlyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questMetadataRepository.GetQuestByIdAsync(id, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            if (existingQuest.QuestType != QuestTypeEnum.Monthly)
                throw new InvalidQuestTypeException(id, QuestTypeEnum.Monthly, existingQuest.QuestType);

            existingQuest.MonthlyQuest!.UpdateDates(updateDto.StartDate, updateDto.EndDate, false);

            _mapper.Map(updateDto, existingQuest.MonthlyQuest);

            var questWithLabels = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest.MonthlyQuest = questWithLabels.MonthlyQuest!;

            await _repository.UpdateAsync(existingQuest.MonthlyQuest, cancellationToken);
        }

        public async Task PatchUserQuestAsync(int id, PatchMonthlyQuestDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingMonthlyQuest = await _repository.GetByIdAsync(id, cancellationToken, dq => dq.QuestMetadata).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            existingMonthlyQuest.UpdateDates(patchDto.StartDate, patchDto.EndDate, false);

            _mapper.Map(patchDto, existingMonthlyQuest);

            await _repository.UpdateAsync(existingMonthlyQuest, cancellationToken);
        }
    }
}
