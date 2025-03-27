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
        private readonly IMapper _mapper;
        private readonly IQuestRepository _questRepository;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public MonthlyQuestService(
            IMapper mapper,
            IQuestRepository questRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _mapper = mapper;
            _questRepository = questRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetMonthlyQuestDto?> GetUserQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await _questRepository.GetQuestByIdAsync(questId, QuestTypeEnum.Monthly, cancellationToken);

            if (quest is null)
                return null;

            //if (quest.QuestType != QuestTypeEnum.Monthly)
            //    throw new InvalidQuestTypeException(questId, QuestTypeEnum.Monthly, quest.QuestType);

            return _mapper.Map<GetMonthlyQuestDto>(quest);
        }

        public async Task<IEnumerable<GetMonthlyQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questRepository.GetQuestsByTypeAsync(accountId, QuestTypeEnum.Monthly, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetMonthlyQuestDto>>(quests);
        }
        public async Task<int> CreateAsync(CreateMonthlyQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var monthlyQuest = _mapper.Map<Quest>(createDto);

            await _questRepository.AddQuestAsync(monthlyQuest, cancellationToken);

            return monthlyQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, UpdateMonthlyQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questRepository.GetQuestByIdAsync(id, QuestTypeEnum.Monthly, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            //if (existingQuest.QuestType != QuestTypeEnum.Monthly)
            //    throw new InvalidQuestTypeException(id, QuestTypeEnum.Monthly, existingQuest.QuestType);

            existingQuest.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            _mapper.Map(updateDto, existingQuest);

            var questWithLabels = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest = questWithLabels;

            await _questRepository.UpdateQuestAsync(existingQuest, cancellationToken);
        }

        public async Task UpdateQuestCompletionAsync(int id, MonthlyQuestCompletionPatchDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingMonthlyQuest = await _questRepository.GetQuestByIdAsync(id, QuestTypeEnum.Monthly, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            _mapper.Map(patchDto, existingMonthlyQuest);

            await _questRepository.UpdateQuestAsync(existingMonthlyQuest, cancellationToken);
        }
    }
}
