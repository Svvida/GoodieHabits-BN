using Application.Dtos.Quests.OneTimeQuest;
using Application.Interfaces;
using Application.Interfaces.Quests;
using AutoMapper;
using Domain.Enum;
using Domain.Exceptions;
using Domain.Interfaces.Quests;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services.Quests
{
    public class OneTimeQuestService : IOneTimeQuestService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<OneTimeQuestService> _logger;
        private readonly IQuestRepository _questRepository;
        private readonly IQuestLabelsHandler _questLabelsHandler;

        public OneTimeQuestService(
            IMapper mapper,
            ILogger<OneTimeQuestService> logger,
            IQuestRepository questRepository,
            IQuestLabelsHandler questLabelsHandler)
        {
            _mapper = mapper;
            _logger = logger;
            _questRepository = questRepository;
            _questLabelsHandler = questLabelsHandler;
        }

        public async Task<GetOneTimeQuestDto?> GetUserQuestByIdAsync(int questId, CancellationToken cancellationToken = default)
        {
            var quest = await _questRepository.GetQuestByIdAsync(questId, QuestTypeEnum.OneTime, cancellationToken);

            if (quest is null)
                return null;

            //if (quest.QuestType != QuestTypeEnum.OneTime)
            //    throw new InvalidQuestTypeException(questId, QuestTypeEnum.OneTime, quest.QuestType);

            return _mapper.Map<GetOneTimeQuestDto>(quest);
        }

        public async Task<IEnumerable<GetOneTimeQuestDto>> GetAllUserQuestsAsync(int accountId, CancellationToken cancellationToken = default)
        {
            var quests = await _questRepository.GetQuestsByTypeAsync(accountId, QuestTypeEnum.OneTime, cancellationToken)
                .ConfigureAwait(false);

            return _mapper.Map<IEnumerable<GetOneTimeQuestDto>>(quests);
        }

        public async Task<int> CreateAsync(CreateOneTimeQuestDto createDto, CancellationToken cancellationToken = default)
        {
            var oneTimeQuest = _mapper.Map<Quest>(createDto);

            await _questRepository.AddQuestAsync(oneTimeQuest, cancellationToken);

            return oneTimeQuest.Id;
        }

        public async Task UpdateUserQuestAsync(int id, UpdateOneTimeQuestDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingQuest = await _questRepository.GetQuestByIdAsync(id, QuestTypeEnum.OneTime, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"Quest with Id {id} was not found.");

            //if (existingQuest.QuestType != QuestTypeEnum.OneTime)
            //    throw new InvalidQuestTypeException(id, QuestTypeEnum.OneTime, existingQuest.QuestType);

            existingQuest.UpdateDates(updateDto.StartDate, updateDto.EndDate);

            _mapper.Map(updateDto, existingQuest);

            var questWithLabels = await _questLabelsHandler.HandleUpdateLabelsAsync(existingQuest, updateDto, cancellationToken).ConfigureAwait(false);
            existingQuest = questWithLabels;

            await _questRepository.UpdateQuestAsync(existingQuest, cancellationToken);
        }

        public async Task UpdateQuestCompletionAsync(int id, OneTimeQuestCompletionDto patchDto, CancellationToken cancellationToken = default)
        {
            var existingOneTimeQuest = await _questRepository.GetQuestByIdAsync(id, QuestTypeEnum.OneTime, cancellationToken).ConfigureAwait(false)
                ?? throw new NotFoundException($"OneTimeQuest with Id {id} was not found.");

            _mapper.Map(patchDto, existingOneTimeQuest);

            await _questRepository.UpdateQuestAsync(existingOneTimeQuest, cancellationToken);
        }
    }
}
